using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using TLCFiTool.JsonRpc;

namespace TLCFiTool.Server;

public sealed class ServerHost
{
    private readonly ConcurrentDictionary<Guid, ClientConnection> _clients = new();
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    public IReadOnlyCollection<ClientConnection> Clients => _clients.Values.ToList();

    public async Task StartAsync(int port, bool useTls, X509Certificate2? serverCert, bool requireClientCert, IReadOnlyCollection<string> allowedThumbprints, JsonRpcDispatcher dispatcher, CancellationToken token)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        while (!_cts.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(_cts.Token);
            var connection = new ClientConnection(client, dispatcher);
            await connection.InitializeAsync(useTls, serverCert, requireClientCert, allowedThumbprints, _cts.Token);
            _clients[connection.Session.SessionId] = connection;
            _ = connection.RunAsync(_cts.Token);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _clients.Clear();
    }
}
