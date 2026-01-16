using System.Text.Json;
using TLCFI.Client;
using TLCFI.JsonRpc;
using TLCFI.Logging;
using TLCFI.Models;
using TLCFI.Server;
using TLCFI.Storage;

namespace TLCFI.UI;

public sealed class MainForm : Form
{
    private readonly ComboBox _modeBox = new();
    private readonly TextBox _hostBox = new();
    private readonly NumericUpDown _portBox = new();
    private readonly TextBox _usernameBox = new();
    private readonly TextBox _passwordBox = new();
    private readonly ComboBox _typeBox = new();
    private readonly NumericUpDown _majorBox = new();
    private readonly NumericUpDown _minorBox = new();
    private readonly NumericUpDown _revisionBox = new();
    private readonly TextBox _uriBox = new();
    private readonly Button _primaryButton = new();
    private readonly Label _statusLabel = new();
    private readonly Label _sessionLabel = new();
    private readonly Label _facilitiesLabel = new();
    private readonly Label _clientsLabel = new();
    private readonly TreeView _objectTree = new();
    private readonly TabControl _tabs = new();
    private readonly TextBox _logBox = new();
    private readonly DataGridView _traceGrid = new();
    private readonly TextBox _rawSendBox = new();

    private readonly UiLogger _logger;
    private readonly AppStorage _storage;
    private readonly ClientHost _client;
    private readonly ServerHost _server;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private CancellationTokenSource? _cts;

    public MainForm()
    {
        Text = "TLC-FI Test Harness";
        Width = 1400;
        Height = 900;

        _storage = new AppStorage();
        _logger = new UiLogger(AppendLog);
        _client = new ClientHost(_logger);
        _server = new ServerHost(_logger, _storage);
        _client.MessageReceived += message => AppendTrace("Rx", message);

        BuildLayout();
        Load += OnLoadAsync;
        FormClosing += OnClosingAsync;
    }

    private async void OnLoadAsync(object? sender, EventArgs e)
    {
        await _server.InitializeAsync().ConfigureAwait(false);
        var profile = await _storage.LoadAsync(_storage.ProfilesPath, new List<ConnectionProfile> { new() }).ConfigureAwait(false);
        var selected = profile.First();
        ApplyProfile(selected);
    }

    private async void OnClosingAsync(object? sender, FormClosingEventArgs e)
    {
        if (_cts is not null)
        {
            _cts.Cancel();
        }

        await _client.DisposeAsync().ConfigureAwait(false);
        await _server.StopAsync().ConfigureAwait(false);
    }

    private void BuildLayout()
    {
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 110 };
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 420 };

        BuildTopPanel(topPanel);
        BuildLeftPanel(split.Panel1);
        BuildRightPanel(split.Panel2);

        Controls.Add(split);
        Controls.Add(topPanel);
    }

    private void BuildTopPanel(Control panel)
    {
        _modeBox.Items.AddRange(new object[] { "Server", "Client" });
        _modeBox.SelectedIndex = 0;
        _modeBox.DropDownStyle = ComboBoxStyle.DropDownList;

        _hostBox.Text = "127.0.0.1";
        _portBox.Value = 11501;
        _portBox.Maximum = 65535;

        _usernameBox.Text = "Chameleon";
        _passwordBox.Text = "CHAM2";
        _passwordBox.UseSystemPasswordChar = true;

        _typeBox.Items.AddRange(new object[] { "Consumer (0)", "Provider (1)", "Control (2)" });
        _typeBox.SelectedIndex = 1;
        _typeBox.DropDownStyle = ComboBoxStyle.DropDownList;

        _majorBox.Value = 1;
        _minorBox.Value = 1;
        _revisionBox.Value = 0;

        _primaryButton.Text = "Start";
        _primaryButton.Width = 90;
        _primaryButton.Click += OnPrimaryClickedAsync;

        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 8, RowCount = 3 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        grid.Controls.Add(new Label { Text = "Mode", AutoSize = true }, 0, 0);
        grid.Controls.Add(_modeBox, 1, 0);
        grid.Controls.Add(new Label { Text = "Host", AutoSize = true }, 2, 0);
        grid.Controls.Add(_hostBox, 3, 0);
        grid.Controls.Add(new Label { Text = "Port", AutoSize = true }, 4, 0);
        grid.Controls.Add(_portBox, 5, 0);
        grid.Controls.Add(_primaryButton, 6, 0);

        grid.Controls.Add(new Label { Text = "User", AutoSize = true }, 0, 1);
        grid.Controls.Add(_usernameBox, 1, 1);
        grid.Controls.Add(new Label { Text = "Pass", AutoSize = true }, 2, 1);
        grid.Controls.Add(_passwordBox, 3, 1);
        grid.Controls.Add(new Label { Text = "Type", AutoSize = true }, 4, 1);
        grid.Controls.Add(_typeBox, 5, 1);

        grid.Controls.Add(new Label { Text = "Ver", AutoSize = true }, 0, 2);
        var versionPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        versionPanel.Controls.Add(_majorBox);
        versionPanel.Controls.Add(new Label { Text = "." });
        versionPanel.Controls.Add(_minorBox);
        versionPanel.Controls.Add(new Label { Text = "." });
        versionPanel.Controls.Add(_revisionBox);
        grid.Controls.Add(versionPanel, 1, 2);

        grid.Controls.Add(new Label { Text = "URI", AutoSize = true }, 2, 2);
        grid.Controls.Add(_uriBox, 3, 2);

        var statusPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        _statusLabel.Text = "Status: idle";
        _sessionLabel.Text = "Session: -";
        _facilitiesLabel.Text = "Facilities: -";
        _clientsLabel.Text = "Clients: 0";
        statusPanel.Controls.AddRange(new Control[] { _statusLabel, _sessionLabel, _facilitiesLabel, _clientsLabel });
        grid.Controls.Add(statusPanel, 7, 0);
        grid.SetRowSpan(statusPanel, 3);

        panel.Controls.Add(grid);
    }

    private void BuildLeftPanel(Control panel)
    {
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 260 };
        _objectTree.Dock = DockStyle.Fill;
        split.Panel1.Controls.Add(_objectTree);

        _tabs.Dock = DockStyle.Fill;
        foreach (var label in new[] { "Session", "Facilities", "Intersection", "SignalGroup", "Detector", "Input", "Output", "SpVeh", "Variables", "Simulator Config" })
        {
            _tabs.TabPages.Add(new TabPage(label) { BackColor = SystemColors.Control });
        }

        split.Panel2.Controls.Add(_tabs);
        panel.Controls.Add(split);
    }

    private void BuildRightPanel(Control panel)
    {
        var rightSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 500 };

        var logPanel = new Panel { Dock = DockStyle.Fill };
        _logBox.Dock = DockStyle.Fill;
        _logBox.Multiline = true;
        _logBox.ReadOnly = true;
        _logBox.ScrollBars = ScrollBars.Vertical;
        logPanel.Controls.Add(_logBox);

        _traceGrid.Dock = DockStyle.Fill;
        _traceGrid.AutoGenerateColumns = false;
        _traceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Time", Width = 120, DataPropertyName = "Time" });
        _traceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dir", Width = 50, DataPropertyName = "Direction" });
        _traceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Method", Width = 120, DataPropertyName = "Method" });
        _traceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", Width = 100, DataPropertyName = "Id" });
        _traceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bytes", Width = 60, DataPropertyName = "Bytes" });
        _traceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Summary", Width = 240, DataPropertyName = "Summary" });
        _traceGrid.DataSource = new BindingSource { DataSource = new List<TraceEntry>() };

        var tracePanel = new Panel { Dock = DockStyle.Fill };
        tracePanel.Controls.Add(_traceGrid);

        var sendPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
        _rawSendBox.Dock = DockStyle.Fill;
        var sendButton = new Button { Text = "Send", Dock = DockStyle.Right, Width = 80 };
        sendButton.Click += async (_, _) => await SendRawAsync();
        sendPanel.Controls.Add(_rawSendBox);
        sendPanel.Controls.Add(sendButton);
        tracePanel.Controls.Add(sendPanel);

        rightSplit.Panel1.Controls.Add(logPanel);
        rightSplit.Panel2.Controls.Add(tracePanel);
        panel.Controls.Add(rightSplit);
    }

    private async void OnPrimaryClickedAsync(object? sender, EventArgs e)
    {
        if (_modeBox.SelectedIndex == 0)
        {
            await ToggleServerAsync().ConfigureAwait(false);
        }
        else
        {
            await ToggleClientAsync().ConfigureAwait(false);
        }
    }

    private async Task ToggleServerAsync()
    {
        if (_cts is null)
        {
            _cts = new CancellationTokenSource();
            _primaryButton.Text = "Stop";
            _statusLabel.Text = "Status: server running";
            _ = Task.Run(() => _server.StartAsync((int)_portBox.Value, _cts.Token));
        }
        else
        {
            _cts.Cancel();
            _cts = null;
            _primaryButton.Text = "Start";
            _statusLabel.Text = "Status: server stopped";
            await _server.StopAsync().ConfigureAwait(false);
        }
    }

    private async Task ToggleClientAsync()
    {
        if (_cts is null)
        {
            _cts = new CancellationTokenSource();
            _primaryButton.Text = "Disconnect";
            _statusLabel.Text = "Status: connecting";
            await _client.ConnectAsync(_hostBox.Text, (int)_portBox.Value, _cts.Token).ConfigureAwait(false);
            await _client.RegisterAsync(_usernameBox.Text, _passwordBox.Text, _typeBox.SelectedIndex, BuildVersion(), _uriBox.Text).ConfigureAwait(false);
            _statusLabel.Text = "Status: connected";
        }
        else
        {
            _cts.Cancel();
            _cts = null;
            _primaryButton.Text = "Connect";
            _statusLabel.Text = "Status: disconnected";
        }
    }

    private async Task SendRawAsync()
    {
        if (string.IsNullOrWhiteSpace(_rawSendBox.Text))
        {
            return;
        }

        if (_modeBox.SelectedIndex == 0)
        {
            foreach (var client in _server.Clients)
            {
                await client.SendAsync(_rawSendBox.Text).ConfigureAwait(false);
            }
        }
        else
        {
            await _client.SendAsync(_rawSendBox.Text).ConfigureAwait(false);
        }
    }

    private void ApplyProfile(ConnectionProfile profile)
    {
        _hostBox.Text = profile.Host;
        _portBox.Value = profile.Port;
        _usernameBox.Text = profile.Username;
        _passwordBox.Text = profile.Password;
        _typeBox.SelectedIndex = profile.Type;
        _majorBox.Value = profile.Version.Major;
        _minorBox.Value = profile.Version.Minor;
        _revisionBox.Value = profile.Version.Revision;
        _uriBox.Text = profile.Uri;
    }

    private AppVersion BuildVersion() => new((int)_majorBox.Value, (int)_minorBox.Value, (int)_revisionBox.Value);

    private void AppendLog(string line)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(line));
            return;
        }

        _logBox.AppendText(line + Environment.NewLine);
    }

    private void AppendTrace(string direction, RpcMessage message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendTrace(direction, message));
            return;
        }

        var list = (BindingSource)_traceGrid.DataSource!;
        var entries = (List<TraceEntry>)list.DataSource!;
        entries.Add(new TraceEntry
        {
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Direction = direction,
            Method = message.Method ?? "response",
            Id = message.Id?.ToString() ?? "-",
            Bytes = message.Root.GetRawText().Length,
            Summary = message.Method ?? "RPC response"
        });
        list.ResetBindings(false);
    }

    private sealed class TraceEntry
    {
        public string Time { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public int Bytes { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
