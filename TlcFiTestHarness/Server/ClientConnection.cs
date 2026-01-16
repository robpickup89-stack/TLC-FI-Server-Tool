using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TLCFI.JsonRpc;
using TLCFI.Logging;
using TLCFI.Transport;

namespace TLCFI.Server;

public sealed class ClientConnection : IAsyncDisposable
{
    private readonly TcpClient _client;
    private readonly NdjsonReader _reader = new();
    private readonly NdjsonWriter _writer;
    private readonly JsonRpcDispatcher _dispatcher = new();
    private readonly RpcMessageParser _parser = new();
    private readonly UiLogger _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly byte[] _buffer = new byte[8192];

    public Guid Id { get; } = Guid.NewGuid();
    public IPEndPoint? Endpoint => _client.Client.RemoteEndPoint as IPEndPoint;
    public DateTime LastMessageUtc { get; private set; } = DateTime.UtcNow;
    public bool Registered { get; set; }
    public string? Username { get; set; }
    public int ClientType { get; set; }
    public string? SessionId { get; set; }

    public event Func<ClientConnection, RpcMessage, Task>? MessageReceived;

    public ClientConnection(TcpClient client, UiLogger logger)
    {
        _client = client;
        _logger = logger;
        _writer = new NdjsonWriter(_client.GetStream());
        _dispatcher.RequestReceived += HandleRequestAsync;
        _dispatcher.NotificationReceived += HandleNotification;
    }

    public async Task RunAsync()
    {
        var stream = _client.GetStream();
        while (!_cts.IsCancellationRequested)
        {
            var read = await stream.ReadAsync(_buffer, _cts.Token).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            LastMessageUtc = DateTime.UtcNow;
            foreach (var json in _reader.ProcessBytes(_buffer.AsSpan(0, read)))
            {
                try
                {
                    var message = _parser.Parse(json);
                    await _dispatcher.DispatchAsync(message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Invalid JSON from {Endpoint}: {ex.Message}");
                }
            }
        }
    }

    private Task<JsonElement?> HandleRequestAsync(RpcMessage message)
    {
        if (MessageReceived is null)
        {
            return Task.FromResult<JsonElement?>(null);
        }

        return MessageReceived.Invoke(this, message)
            .ContinueWith(_ => (JsonElement?)null, TaskScheduler.Default);
    }

    private void HandleNotification(RpcMessage message)
    {
        MessageReceived?.Invoke(this, message);
    }

    public Task SendAsync(string json, CancellationToken cancellationToken = default)
    {
        return _writer.EnqueueAsync(json, cancellationToken).AsTask();
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _client.Close();
        await _writer.DisposeAsync().ConfigureAwait(false);
    }
}
