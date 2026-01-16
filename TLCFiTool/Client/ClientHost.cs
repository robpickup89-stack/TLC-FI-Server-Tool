using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using TLCFiTool.Auth;
using TLCFiTool.JsonRpc;
using TLCFiTool.Transport;

namespace TLCFiTool.Client;

public sealed class ClientHost
{
    private TcpClient? _client;
    private Stream? _stream;

    public async Task ConnectAsync(string host, int port, bool useTls, X509Certificate2? clientCert, bool allowSelfSigned, CancellationToken token)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(host, port, token);

        var networkStream = _client.GetStream();
        if (!useTls)
        {
            _stream = networkStream;
            return;
        }

        var sslStream = new SslStream(networkStream, false, (sender, cert, chain, errors) =>
            TlsAuth.ValidateServerCertificate(allowSelfSigned, sender, cert, chain, errors));

        var certs = new X509CertificateCollection();
        if (clientCert is not null)
        {
            certs.Add(clientCert);
        }

        await sslStream.AuthenticateAsClientAsync(host, certs, false);
        _stream = sslStream;
    }

    public async Task RunAsync(Func<JsonRpcMessage, Task> onMessage, CancellationToken token)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Not connected.");
        }

        var reader = new StreamReadLoop(_stream, Encoding.UTF8);
        await reader.ReadAsync(async line =>
        {
            var message = JsonSerializer.Deserialize<JsonRpcMessage>(line);
            if (message is not null)
            {
                await onMessage(message);
            }
        }, token);
    }
}
