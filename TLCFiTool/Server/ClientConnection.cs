using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using TLCFiTool.Auth;
using TLCFiTool.JsonRpc;
using TLCFiTool.Models;
using TLCFiTool.Transport;

namespace TLCFiTool.Server;

public sealed class ClientConnection
{
    private readonly TcpClient _client;
    private readonly JsonRpcDispatcher _dispatcher;
    private Stream? _stream;

    public ClientConnection(TcpClient client, JsonRpcDispatcher dispatcher)
    {
        _client = client;
        _dispatcher = dispatcher;
    }

    public Session Session { get; } = new();
    public SubscriptionManager Subscriptions { get; } = new();

    public async Task InitializeAsync(bool useTls, X509Certificate2? serverCert, bool requireClientCert, IReadOnlyCollection<string> allowedThumbprints, CancellationToken token)
    {
        var networkStream = _client.GetStream();
        if (!useTls)
        {
            _stream = networkStream;
            return;
        }

        var sslStream = new SslStream(networkStream, false, (sender, cert, chain, errors) =>
            TlsAuth.ValidateServerCertificate(true, sender, cert, chain, errors));
        await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
        {
            ServerCertificate = serverCert,
            ClientCertificateRequired = requireClientCert,
            CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
        }, token);

        if (requireClientCert && !TlsAuth.IsClientCertificateAllowed(sslStream.RemoteCertificate as X509Certificate2, allowedThumbprints))
        {
            throw new InvalidOperationException("Client certificate not allowed.");
        }

        _stream = sslStream;
    }

    public async Task RunAsync(CancellationToken token)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Stream not initialized.");
        }

        var reader = new StreamReadLoop(_stream, Encoding.UTF8);
        var writer = new StreamWriteQueue(_stream, Encoding.UTF8);
        _ = writer.RunAsync(token);

        await reader.ReadAsync(async line =>
        {
            var message = JsonSerializer.Deserialize<JsonRpcMessage>(line);
            if (message is null)
            {
                return;
            }

            var response = await _dispatcher.DispatchAsync(message);
            var payload = JsonSerializer.Serialize(response);
            await writer.EnqueueAsync(payload, token);
        }, token);
    }
}
