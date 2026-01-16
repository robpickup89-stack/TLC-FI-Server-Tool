using System.Net;
using System.Text.Json;
using TLCFI.JsonRpc;
using TLCFI.Logging;
using TLCFI.Models;
using TLCFI.Storage;

namespace TLCFI.Server;

public sealed class ServerHost
{
    private readonly UiLogger _logger;
    private readonly AuthService _auth;
    private readonly SimulatorStateStore _state;
    private readonly SubscriptionManager _subscriptions;
    private readonly AppStorage _storage;
    private TcpListener? _listener;
    private readonly List<ClientConnection> _clients = [];
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);
    private CancellationTokenSource? _cts;
    private SimulatorConfig _config = new();
    private int _sessionCounter = 0;
    private ulong _ticks;

    public int Port { get; private set; } = 11501;
    public IReadOnlyList<ClientConnection> Clients => _clients;

    public ServerHost(UiLogger logger, AppStorage storage)
    {
        _logger = logger;
        _storage = storage;
        _auth = new AuthService(storage);
        _state = new SimulatorStateStore();
        _subscriptions = new SubscriptionManager();
    }

    public async Task InitializeAsync()
    {
        await _auth.InitializeAsync().ConfigureAwait(false);
        _config = await _storage.LoadAsync(_storage.ConfigPath, SimulatorDefaults.Build()).ConfigureAwait(false);
        _ticks = _config.Settings.TickStart;
        SimulatorDefaults.ApplyDefaults(_config, _state);
    }

    public async Task StartAsync(int port, CancellationToken cancellationToken)
    {
        Port = port;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _logger.Info($"Server listening on {port}");

        _ = Task.Run(() => BroadcastLoopAsync(_cts.Token));

        while (!_cts.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(_cts.Token).ConfigureAwait(false);
            var connection = new ClientConnection(client, _logger);
            connection.MessageReceived += HandleMessageAsync;
            _clients.Add(connection);
            _ = Task.Run(connection.RunAsync, _cts.Token);
        }
    }

    public async Task StopAsync()
    {
        if (_cts is not null)
        {
            _cts.Cancel();
        }

        foreach (var client in _clients.ToArray())
        {
            await client.DisposeAsync().ConfigureAwait(false);
        }

        _clients.Clear();
        _listener?.Stop();
    }

    private async Task HandleMessageAsync(ClientConnection connection, RpcMessage message)
    {
        if (message.Method is null)
        {
            return;
        }

        if (!connection.Registered && !string.Equals(message.Method, "Register", StringComparison.OrdinalIgnoreCase))
        {
            await SendErrorAsync(connection, message.Id, RpcError.NotRegistered()).ConfigureAwait(false);
            return;
        }

        switch (message.Method)
        {
            case "Register":
                await HandleRegisterAsync(connection, message).ConfigureAwait(false);
                break;
            case "Deregister":
                await HandleDeregisterAsync(connection, message).ConfigureAwait(false);
                break;
            case "ReadMeta":
                await HandleReadMetaAsync(connection, message).ConfigureAwait(false);
                break;
            case "Subscribe":
                await HandleSubscribeAsync(connection, message).ConfigureAwait(false);
                break;
            case "UpdateState":
                await HandleUpdateStateAsync(connection, message).ConfigureAwait(false);
                break;
        }
    }

    private async Task HandleRegisterAsync(ClientConnection connection, RpcMessage message)
    {
        if (message.Params is null)
        {
            await SendErrorAsync(connection, message.Id, RpcError.IncorrectCredentials()).ConfigureAwait(false);
            return;
        }

        var username = message.Params.Value.GetProperty("username").GetString() ?? string.Empty;
        var password = message.Params.Value.GetProperty("password").GetString() ?? string.Empty;
        var type = message.Params.Value.GetProperty("type").GetInt32();
        var version = message.Params.Value.GetProperty("version");

        var user = _auth.Validate(username, password, type);
        if (user is null)
        {
            await SendErrorAsync(connection, message.Id, RpcError.IncorrectCredentials()).ConfigureAwait(false);
            return;
        }

        connection.Registered = true;
        connection.Username = username;
        connection.ClientType = type;
        connection.SessionId = string.Format(_config.SessionIdTemplate, Interlocked.Increment(ref _sessionCounter));

        var result = new
        {
            sessionid = connection.SessionId,
            facilities = new { type = 1, ids = new[] { _config.FacilitiesId } },
            version = new
            {
                major = version.GetProperty("major").GetInt32(),
                minor = version.GetProperty("minor").GetInt32(),
                revision = version.GetProperty("revision").GetInt32()
            }
        };

        await SendResultAsync(connection, message.Id, result).ConfigureAwait(false);
    }

    private async Task HandleDeregisterAsync(ClientConnection connection, RpcMessage message)
    {
        await SendResultAsync(connection, message.Id, new { deregistered = true }).ConfigureAwait(false);
        await connection.DisposeAsync().ConfigureAwait(false);
    }

    private Task HandleReadMetaAsync(ClientConnection connection, RpcMessage message)
    {
        var type = message.Params?.Value.GetProperty("type").GetInt32() ?? 0;
        var ids = message.Params?.Value.GetProperty("ids").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray() ?? [];

        var meta = _state.Meta.Where(kv => kv.Key.Type == type && ids.Contains(kv.Key.Id)).Select(kv => kv.Value).ToArray();
        var result = new
        {
            objects = new { type, ids },
            meta,
            ticks = _ticks
        };

        return SendResultAsync(connection, message.Id, result);
    }

    private Task HandleSubscribeAsync(ClientConnection connection, RpcMessage message)
    {
        var type = message.Params?.Value.GetProperty("type").GetInt32() ?? 0;
        var ids = message.Params?.Value.GetProperty("ids").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray() ?? [];
        _subscriptions.UpdateSubscription(connection.Id, type, ids);

        var data = _state.State.Where(kv => kv.Key.Type == type && ids.Contains(kv.Key.Id)).Select(kv => kv.Value).ToArray();
        var result = new
        {
            objects = new { type, ids },
            data,
            ticks = _ticks
        };

        return SendResultAsync(connection, message.Id, result);
    }

    private Task HandleUpdateStateAsync(ClientConnection connection, RpcMessage message)
    {
        if (connection.ClientType == 0)
        {
            return SendErrorAsync(connection, message.Id, RpcError.PermissionDenied());
        }

        _logger.Info($"UpdateState received from {connection.Username}");
        return Task.CompletedTask;
    }

    private async Task BroadcastLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _ticks += 25;
            var payload = SimulatorDefaults.BuildUpdateState(_state, _subscriptions, _clients, _ticks);
            foreach (var client in _clients.ToArray())
            {
                if (payload.TryGetValue(client.Id, out var json))
                {
                    await client.SendAsync(json, cancellationToken).ConfigureAwait(false);
                }
            }

            await Task.Delay(_config.Settings.UpdateIntervalMs, cancellationToken).ConfigureAwait(false);
        }
    }

    private Task SendResultAsync(ClientConnection connection, JsonElement? id, object result)
    {
        if (id is null)
        {
            return Task.CompletedTask;
        }

        var response = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id.Value,
            ["result"] = result
        };

        var json = JsonSerializer.Serialize(response, _options);
        return connection.SendAsync(json);
    }

    private Task SendErrorAsync(ClientConnection connection, JsonElement? id, RpcError error)
    {
        if (id is null)
        {
            return Task.CompletedTask;
        }

        var response = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id.Value,
            ["error"] = new { code = error.Code, message = error.Message }
        };

        var json = JsonSerializer.Serialize(response, _options);
        return connection.SendAsync(json);
    }
}
