using System;
using System.Drawing;
using System.Windows.Forms;

namespace TLCFiTool.UI;

public sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "TLC-FI Test Harness";
        MinimumSize = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;

        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var topPanel = BuildTopPanel();
        var mainSplit = BuildMainSplit();

        rootLayout.Controls.Add(topPanel, 0, 0);
        rootLayout.Controls.Add(mainSplit, 0, 1);

        Controls.Add(rootLayout);
    }

    private Control BuildTopPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 190,
            Padding = new Padding(10),
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 4,
        };

        for (var i = 0; i < layout.ColumnCount; i++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66f));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        layout.Controls.Add(CreateLabeledCombo("Mode", new[] { "Server", "Client" }), 0, 0);
        layout.Controls.Add(CreateLabeledText("Host", "localhost"), 1, 0);
        layout.Controls.Add(CreateLabeledNumeric("Port", 11501, 1, 65535), 2, 0);
        layout.Controls.Add(CreateLabeledCombo("Auth Mode", new[] { "TLS (mTLS)", "Username/Password" }), 3, 0);
        layout.Controls.Add(CreateLabeledCombo("App Type", new[] { "Consumer (0)", "Provider (1)", "Control (2)" }), 4, 0);
        layout.Controls.Add(CreateLabeledButton("Start / Connect"), 5, 0);

        layout.Controls.Add(CreateLabeledText("Server PFX", ""), 0, 1);
        layout.Controls.Add(CreateLabeledText("Server PFX Password", ""), 1, 1);
        layout.Controls.Add(CreateLabeledText("Client PFX", ""), 2, 1);
        layout.Controls.Add(CreateLabeledText("Client PFX Password", ""), 3, 1);
        layout.Controls.Add(CreateLabeledCheckbox("Require Client Cert", true), 4, 1);
        layout.Controls.Add(CreateLabeledCheckbox("Allow Self-Signed", false), 5, 1);

        layout.Controls.Add(CreateLabeledText("Username", ""), 0, 2);
        layout.Controls.Add(CreateLabeledText("Password", ""), 1, 2);
        layout.Controls.Add(CreateLabeledNumeric("Version", 1, 0, 9999), 2, 2);
        layout.Controls.Add(CreateLabeledNumeric("Revision", 0, 0, 9999), 3, 2);
        layout.Controls.Add(CreateLabeledText("URI", "127.0.0.1"), 4, 2);
        var statusPanel = CreateStatusPanel();
        layout.Controls.Add(statusPanel, 0, 3);
        layout.SetColumnSpan(statusPanel, 6);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control BuildMainSplit()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 800,
        };

        split.Panel1.Controls.Add(BuildLeftTabs());
        split.Panel2.Controls.Add(BuildDebugPanel());

        return split;
    }

    private Control BuildLeftTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
        };

        tabs.TabPages.Add(BuildDetectorsTab());
        tabs.TabPages.Add(BuildOutputsTab());
        tabs.TabPages.Add(BuildIntersectionsTab());
        tabs.TabPages.Add(BuildControlTab());
        tabs.TabPages.Add(BuildScriptsTab());

        return tabs;
    }

    private TabPage BuildDetectorsTab()
    {
        var page = new TabPage("Detectors");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        buttonPanel.Controls.AddRange(new Control[]
        {
            new Button { Text = "Toggle" },
            new Button { Text = "Set Occupied" },
            new Button { Text = "Set Unoccupied" },
            new Button { Text = "Pulse" },
            new Button { Text = "Random Generator" },
        });

        var grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        grid.Columns.Add("Index", "Index");
        grid.Columns.Add("Name", "Name");
        grid.Columns.Add("GeneratesEvents", "GeneratesEvents");
        grid.Columns.Add("State", "State");
        grid.Columns.Add("FaultState", "FaultState");
        grid.Columns.Add("Swico", "Swico");
        grid.Columns.Add("StateTicks", "StateTicks");
        grid.Columns.Add("Notes", "Notes");

        layout.Controls.Add(buttonPanel, 0, 0);
        layout.Controls.Add(grid, 0, 1);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildOutputsTab()
    {
        var page = new TabPage("Outputs");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        buttonPanel.Controls.AddRange(new Control[]
        {
            new Button { Text = "Apply Req → State" },
            new Button { Text = "Clear Req" },
            new Button { Text = "Bulk Set" },
        });

        var grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        grid.Columns.Add("Index", "Index");
        grid.Columns.Add("Name", "Name");
        grid.Columns.Add("State", "State");
        grid.Columns.Add("ReqState", "ReqState");
        grid.Columns.Add("FaultState", "FaultState");
        grid.Columns.Add("StateTicks", "StateTicks");
        grid.Columns.Add("Notes", "Notes");

        layout.Controls.Add(buttonPanel, 0, 0);
        layout.Controls.Add(grid, 0, 1);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildIntersectionsTab()
    {
        var page = new TabPage("Intersections");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        buttonPanel.Controls.AddRange(new Control[]
        {
            new Button { Text = "Add Intersection" },
            new Button { Text = "Assign Detectors" },
            new Button { Text = "Assign Outputs" },
        });

        var grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        grid.Columns.Add("IntersectionId", "Intersection Id");
        grid.Columns.Add("Name", "Name");
        grid.Columns.Add("Detectors", "Detectors");
        grid.Columns.Add("Outputs", "Outputs");
        grid.Columns.Add("Notes", "Notes");

        layout.Controls.Add(buttonPanel, 0, 0);
        layout.Controls.Add(grid, 0, 1);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildControlTab()
    {
        var page = new TabPage("Control");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var controlPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };

        controlPanel.Controls.AddRange(new Control[]
        {
            new Label { Text = "Control State:", AutoSize = true, Padding = new Padding(0, 10, 0, 0) },
            new TextBox { Width = 200, ReadOnly = true },
            new Label { Text = "Req Control State:", AutoSize = true, Padding = new Padding(0, 10, 0, 0) },
            new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Items = { "NotConfigured", "Offline", "ReadyToControl", "StartControl", "InControl", "EndControl", "Error" } },
            new Button { Text = "ReadyToControl" },
            new Button { Text = "StartControl" },
            new Button { Text = "EndControl" },
            new Button { Text = "Build Atomic Update" },
        });

        var grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        grid.Columns.Add("ObjectType", "Object Type");
        grid.Columns.Add("ObjectId", "Object Id");
        grid.Columns.Add("Attribute", "Attribute");
        grid.Columns.Add("Value", "Value");

        layout.Controls.Add(controlPanel, 0, 0);
        layout.Controls.Add(grid, 0, 1);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildScriptsTab()
    {
        var page = new TabPage("Scripts");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        buttonPanel.Controls.AddRange(new Control[]
        {
            new Button { Text = "Load Script" },
            new Button { Text = "Save Script" },
            new Button { Text = "Start" },
            new Button { Text = "Stop" },
        });

        var scriptEditor = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Consolas", 10f),
        };

        layout.Controls.Add(buttonPanel, 0, 0);
        layout.Controls.Add(scriptEditor, 0, 1);

        page.Controls.Add(layout);
        return page;
    }

    private Control BuildDebugPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

        var logBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            Font = new Font("Consolas", 9f),
        };

        var traceGrid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        traceGrid.Columns.Add("Time", "Time");
        traceGrid.Columns.Add("Direction", "Dir");
        traceGrid.Columns.Add("Method", "Method");
        traceGrid.Columns.Add("Bytes", "Bytes");
        traceGrid.Columns.Add("Summary", "Summary");

        var rawPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
        };
        rawPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        rawPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

        var rawText = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Consolas", 9f),
        };

        var rawButtons = new FlowLayoutPanel { Dock = DockStyle.Fill };
        rawButtons.Controls.AddRange(new Control[]
        {
            new Button { Text = "Send" },
            new Button { Text = "Pretty Print" },
            new Button { Text = "Validate JSON" },
            new Button { Text = "Export Trace" },
            new Button { Text = "Replay Trace" },
        });

        rawPanel.Controls.Add(rawText, 0, 0);
        rawPanel.Controls.Add(rawButtons, 0, 1);

        layout.Controls.Add(logBox, 0, 0);
        layout.Controls.Add(traceGrid, 0, 1);
        layout.Controls.Add(rawPanel, 0, 2);

        var debugGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Debug",
        };
        debugGroup.Controls.Add(layout);

        return debugGroup;
    }

    private static Control CreateStatusPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
        };

        panel.Controls.AddRange(new Control[]
        {
            new Label { Text = "Server: Stopped", AutoSize = true },
            new Label { Text = "Client: Disconnected", AutoSize = true },
            new Label { Text = "Auth: No", AutoSize = true },
            new Label { Text = "Session: -", AutoSize = true },
            new Label { Text = "Clients: 0", AutoSize = true },
        });

        return panel;
    }

    private static Control CreateLabeledText(string label, string value)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var caption = new Label { Text = label, Dock = DockStyle.Top, AutoSize = true };
        var box = new TextBox { Dock = DockStyle.Fill, Text = value };
        panel.Controls.Add(box);
        panel.Controls.Add(caption);
        return panel;
    }

    private static Control CreateLabeledNumeric(string label, int value, int min, int max)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var caption = new Label { Text = label, Dock = DockStyle.Top, AutoSize = true };
        var input = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = min,
            Maximum = max,
            Value = value,
        };
        panel.Controls.Add(input);
        panel.Controls.Add(caption);
        return panel;
    }

    private static Control CreateLabeledCombo(string label, string[] options)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var caption = new Label { Text = label, Dock = DockStyle.Top, AutoSize = true };
        var combo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };
        combo.Items.AddRange(options);
        if (options.Length > 0)
        {
            combo.SelectedIndex = 0;
        }
        panel.Controls.Add(combo);
        panel.Controls.Add(caption);
        return panel;
    }

    private static Control CreateLabeledCheckbox(string label, bool isChecked)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var checkbox = new CheckBox { Text = label, Checked = isChecked, Dock = DockStyle.Fill };
        panel.Controls.Add(checkbox);
        return panel;
    }

    private static Control CreateLabeledButton(string label)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var button = new Button { Text = label, Dock = DockStyle.Fill };
        panel.Controls.Add(button);
        return panel;
    }
}
