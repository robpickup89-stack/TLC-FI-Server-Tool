using System.Net.Sockets;
using System.Text.Json;
using TLCFI.JsonRpc;
using TLCFI.Logging;
using TLCFI.Models;
using TLCFI.Transport;

namespace TLCFI.Client;

public sealed class ClientHost : IAsyncDisposable
{
    private readonly UiLogger _logger;
    private readonly ClientProtocol _protocol = new();
    private readonly RpcMessageParser _parser = new();
    private readonly NdjsonReader _reader = new();
    private NdjsonWriter? _writer;
    private TcpClient? _client;
    private readonly byte[] _buffer = new byte[8192];
    private CancellationTokenSource? _cts;

    public ClientStateStore StateStore { get; } = new();
    public SessionInfo? Session { get; private set; }

    public event Action<RpcMessage>? MessageReceived;

    public ClientHost(UiLogger logger)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
        _writer = new NdjsonWriter(_client.GetStream());
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    public async Task RegisterAsync(string username, string password, int type, AppVersion version, string uri)
    {
        await SendAsync(_protocol.BuildRegister("msgid22", username, password, type, version, uri)).ConfigureAwait(false);
    }

    public Task SendAsync(string json)
    {
        if (_writer is null)
        {
            return Task.CompletedTask;
        }

        return _writer.EnqueueAsync(json).AsTask();
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        if (_client is null)
        {
            return;
        }

        var stream = _client.GetStream();
        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await stream.ReadAsync(_buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            foreach (var json in _reader.ProcessBytes(_buffer.AsSpan(0, read)))
            {
                try
                {
                    var message = _parser.Parse(json);
                    MessageReceived?.Invoke(message);
                    HandleResponse(message);
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Invalid JSON: {ex.Message}");
                }
            }
        }
    }

    private void HandleResponse(RpcMessage message)
    {
        if (message.Result is null)
        {
            return;
        }

        if (message.Result.Value.TryGetProperty("sessionid", out var sessionId))
        {
            var facilities = message.Result.Value.GetProperty("facilities");
            var ids = facilities.GetProperty("ids").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();
            var version = message.Result.Value.GetProperty("version");
            Session = new SessionInfo(
                sessionId.GetString() ?? string.Empty,
                ids,
                new AppVersion(version.GetProperty("major").GetInt32(), version.GetProperty("minor").GetInt32(), version.GetProperty("revision").GetInt32()),
                facilities.GetProperty("type").GetInt32());
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cts is not null)
        {
            _cts.Cancel();
        }

        if (_writer is not null)
        {
            await _writer.DisposeAsync().ConfigureAwait(false);
        }

        _client?.Close();
    }
}
