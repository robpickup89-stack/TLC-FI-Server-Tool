using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TLCFiServerTool;

public sealed class MainForm : Form
{
    private readonly TextBox _portTextBox = new();
    private readonly Button _startButton = new();
    private readonly Button _stopButton = new();
    private readonly Button _sendButton = new();
    private readonly TextBox _sendTextBox = new();
    private readonly TextBox _logTextBox = new();
    private readonly ListBox _clientsListBox = new();
    private readonly CheckBox _autoEchoCheckBox = new();
    private readonly CheckBox _hexDisplayCheckBox = new();
    private readonly CheckBox _autoResponseCheckBox = new();
    private readonly Label _statusLabel = new();

    private readonly ConcurrentDictionary<int, TcpClient> _clients = new();
    private CancellationTokenSource? _listenerCts;
    private TcpListener? _listener;
    private int _clientCounter;

    public MainForm()
    {
        Text = "TLC-Fi Server Tool";
        Width = 980;
        Height = 720;
        MinimumSize = new System.Drawing.Size(980, 720);

        var portLabel = new Label
        {
            Text = "Port",
            Left = 16,
            Top = 18,
            Width = 40
        };

        _portTextBox.Left = 60;
        _portTextBox.Top = 14;
        _portTextBox.Width = 80;
        _portTextBox.Text = "8500";

        _startButton.Text = "Start";
        _startButton.Left = 160;
        _startButton.Top = 12;
        _startButton.Width = 80;
        _startButton.Click += (_, _) => StartServer();

        _stopButton.Text = "Stop";
        _stopButton.Left = 248;
        _stopButton.Top = 12;
        _stopButton.Width = 80;
        _stopButton.Enabled = false;
        _stopButton.Click += (_, _) => StopServer();

        _statusLabel.Left = 350;
        _statusLabel.Top = 16;
        _statusLabel.Width = 600;
        _statusLabel.Text = "Stopped";

        _clientsListBox.Left = 16;
        _clientsListBox.Top = 52;
        _clientsListBox.Width = 310;
        _clientsListBox.Height = 520;

        var clientLabel = new Label
        {
            Text = "Connected Clients",
            Left = 16,
            Top = 32,
            Width = 200
        };

        _logTextBox.Left = 340;
        _logTextBox.Top = 52;
        _logTextBox.Width = 610;
        _logTextBox.Height = 520;
        _logTextBox.Multiline = true;
        _logTextBox.ReadOnly = true;
        _logTextBox.ScrollBars = ScrollBars.Vertical;
        _logTextBox.Font = new System.Drawing.Font("Consolas", 9);

        _sendTextBox.Left = 16;
        _sendTextBox.Top = 590;
        _sendTextBox.Width = 780;
        _sendTextBox.Height = 60;
        _sendTextBox.Multiline = true;

        _sendButton.Text = "Send";
        _sendButton.Left = 810;
        _sendButton.Top = 590;
        _sendButton.Width = 140;
        _sendButton.Height = 60;
        _sendButton.Enabled = false;
        _sendButton.Click += (_, _) => SendToSelectedClient();

        _autoEchoCheckBox.Text = "Auto-echo received data";
        _autoEchoCheckBox.Left = 16;
        _autoEchoCheckBox.Top = 654;
        _autoEchoCheckBox.Width = 200;

        _hexDisplayCheckBox.Text = "Show hex payloads";
        _hexDisplayCheckBox.Left = 230;
        _hexDisplayCheckBox.Top = 654;
        _hexDisplayCheckBox.Width = 160;
        _hexDisplayCheckBox.Checked = true;

        _autoResponseCheckBox.Text = "Auto-respond to TLC-FI version checks";
        _autoResponseCheckBox.Left = 400;
        _autoResponseCheckBox.Top = 654;
        _autoResponseCheckBox.Width = 320;
        _autoResponseCheckBox.Checked = true;

        Controls.Add(portLabel);
        Controls.Add(_portTextBox);
        Controls.Add(_startButton);
        Controls.Add(_stopButton);
        Controls.Add(_statusLabel);
        Controls.Add(clientLabel);
        Controls.Add(_clientsListBox);
        Controls.Add(_logTextBox);
        Controls.Add(_sendTextBox);
        Controls.Add(_sendButton);
        Controls.Add(_autoEchoCheckBox);
        Controls.Add(_hexDisplayCheckBox);
        Controls.Add(_autoResponseCheckBox);
    }

    private void StartServer()
    {
        if (!int.TryParse(_portTextBox.Text, out var port) || port <= 0 || port > 65535)
        {
            MessageBox.Show("Enter a valid port number.", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_listenerCts is not null)
        {
            return;
        }

        _listenerCts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        _startButton.Enabled = false;
        _stopButton.Enabled = true;
        _sendButton.Enabled = true;
        _statusLabel.Text = $"Listening on 0.0.0.0:{port}";

        LogMessage($"Server started on port {port}.");

        _ = Task.Run(() => AcceptLoopAsync(_listener, _listenerCts.Token));
    }

    private void StopServer()
    {
        if (_listenerCts is null)
        {
            return;
        }

        _listenerCts.Cancel();
        _listenerCts.Dispose();
        _listenerCts = null;

        _listener?.Stop();
        _listener = null;

        foreach (var client in _clients.Values)
        {
            client.Close();
        }

        _clients.Clear();
        _clientsListBox.Items.Clear();

        _startButton.Enabled = true;
        _stopButton.Enabled = false;
        _sendButton.Enabled = false;
        _statusLabel.Text = "Stopped";

        LogMessage("Server stopped.");
    }

    private async Task AcceptLoopAsync(TcpListener listener, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TcpClient? client = null;
            try
            {
                client = await listener.AcceptTcpClientAsync(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                LogMessage($"Accept error: {ex.Message}");
            }

            if (client is null)
            {
                continue;
            }

            var clientId = Interlocked.Increment(ref _clientCounter);
            _clients[clientId] = client;

            Invoke(() =>
            {
                _clientsListBox.Items.Add(ClientDisplayName(clientId, client));
            });

            LogMessage($"Client #{clientId} connected from {client.Client.RemoteEndPoint}.");

            _ = Task.Run(() => ReceiveLoopAsync(clientId, client, token));
        }
    }

    private async Task ReceiveLoopAsync(int clientId, TcpClient client, CancellationToken token)
    {
        var buffer = new byte[4096];
        NetworkStream? stream = null;

        try
        {
            stream = client.GetStream();
            while (!token.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, token);
                if (bytesRead == 0)
                {
                    break;
                }

                var payload = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var hex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", " ");
                var formatted = _hexDisplayCheckBox.Checked
                    ? $"RX #{clientId} ({bytesRead} bytes) {hex} | {payload}"
                    : $"RX #{clientId} ({bytesRead} bytes) {payload}";

                LogMessage(formatted);

                var autoResponded = await TrySendAutoResponseAsync(clientId, stream, payload, token);
                if (_autoEchoCheckBox.Checked && !autoResponded)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                    LogMessage($"Echoed {bytesRead} bytes to #{clientId}.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            LogMessage($"Client #{clientId} receive canceled.");
        }
        catch (Exception ex)
        {
            LogMessage($"Client #{clientId} receive error: {ex.Message}");
        }
        finally
        {
            stream?.Close();
            client.Close();
            _clients.TryRemove(clientId, out _);

            Invoke(() => RemoveClientFromList(clientId));
            LogMessage($"Client #{clientId} disconnected.");
        }
    }

    private async Task<bool> TrySendAutoResponseAsync(int clientId, NetworkStream stream, string payload, CancellationToken token)
    {
        if (!_autoResponseCheckBox.Checked)
        {
            return false;
        }

        foreach (var (key, response) in TlcFiPayloads.Responses)
        {
            if (!payload.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var data = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(data, token);
            LogMessage($"Auto-response for {key} sent to #{clientId} ({data.Length} bytes).");
            return true;
        }

        return false;
    }

    private void SendToSelectedClient()
    {
        if (_clientsListBox.SelectedItem is not string selected)
        {
            MessageBox.Show("Select a client to send data.", "No Client Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!TryParseClientId(selected, out var clientId) || !_clients.TryGetValue(clientId, out var client))
        {
            MessageBox.Show("Selected client is no longer available.", "Client Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var message = _sendTextBox.Text;
        if (string.IsNullOrWhiteSpace(message))
        {
            MessageBox.Show("Enter a message to send.", "No Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var data = Encoding.UTF8.GetBytes(message);
            client.GetStream().Write(data, 0, data.Length);
            LogMessage($"TX #{clientId} ({data.Length} bytes) {message}");
        }
        catch (Exception ex)
        {
            LogMessage($"Send error to #{clientId}: {ex.Message}");
        }
    }

    private void LogMessage(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var line = $"[{timestamp}] {message}";

        if (InvokeRequired)
        {
            Invoke(() => AppendLog(line));
        }
        else
        {
            AppendLog(line);
        }
    }

    private void AppendLog(string line)
    {
        _logTextBox.AppendText(line + Environment.NewLine);
    }

    private static string ClientDisplayName(int clientId, TcpClient client)
    {
        return $"#{clientId} - {client.Client.RemoteEndPoint}";
    }

    private void RemoveClientFromList(int clientId)
    {
        for (var i = _clientsListBox.Items.Count - 1; i >= 0; i--)
        {
            if (_clientsListBox.Items[i] is string item && item.StartsWith($"#{clientId} ", StringComparison.Ordinal))
            {
                _clientsListBox.Items.RemoveAt(i);
            }
        }
    }

    private static bool TryParseClientId(string display, out int clientId)
    {
        clientId = 0;
        if (!display.StartsWith('#'))
        {
            return false;
        }

        var endIndex = display.IndexOf(' ');
        if (endIndex <= 1)
        {
            return false;
        }

        return int.TryParse(display[1..endIndex], out clientId);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopServer();
        base.OnFormClosing(e);
    }
}
