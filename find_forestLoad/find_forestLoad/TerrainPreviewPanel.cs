using System.Globalization;
using find_forestLoad.Las;

namespace find_forestLoad;

internal enum TerrainPickKind
{
    Start,
    End
}

internal readonly record struct TerrainPointPick(TerrainPickKind Kind, double X, double Y, double? Z);

internal sealed class TerrainPreviewPanel : UserControl
{
    private readonly TerrainMapControl _map;
    private readonly Label _header;
    private readonly Label _status;
    private readonly Label _minLabel;
    private readonly Label _maxLabel;
    private readonly Panel _ramp;
    private readonly CheckBox _markers;
    private readonly FlowLayoutPanel _routes;
    private readonly Dictionary<RoadObjective, CheckBox> _routeChecks = new();
    private readonly Button _pickStart;
    private readonly Button _pickEnd;
    private readonly Button _showAll;
    private TerrainPickKind? _pick;
    private RoadObjective? _emphasized;

    private ElevationGrid? _grid;
    private TerrainOverview _overview;
    private Bitmap? _overviewBitmap;
    private IReadOnlyDictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>> _profiles =
        new Dictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>>();
    private (double X, double Y)? _start;
    private (double X, double Y)? _end;

    public TerrainPreviewPanel()
    {
        Font = new Font("맑은 고딕", 9.75F);
        BackColor = Color.FromArgb(245, 240, 238);

        _header = new Label
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 8, 0),
            ForeColor = Color.FromArgb(70, 70, 70),
            Text = "지형 보기를 누르면 고도가 여기에 표시됩니다."
        };

        _map = new TerrainMapControl(this) { Dock = DockStyle.Fill };

        _status = new Label
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 8, 0),
            ForeColor = Color.FromArgb(60, 60, 60),
            Text = TerrainMapControl.Hint
        };
        _map.HoverTextChanged += text => _status.Text = text;
        _map.ViewChanged += UpdateStatusHint;

        _maxLabel = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };
        _minLabel = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0)
        };
        _ramp = new Panel
        {
            Size = new Size(36, 150),
            Margin = new Padding(0, 0, 0, 0)
        };
        _ramp.Paint += DrawRamp;

        _markers = new CheckBox
        {
            Text = "시점 · 종점",
            Checked = true,
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 4)
        };
        _markers.CheckedChanged += (_, _) => _map.Invalidate();

        _routes = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 4, 0, 0),
            MaximumSize = new Size(176, 400)
        };

        _pickStart = CreatePickButton("시점 찍기");
        _pickEnd = CreatePickButton("종점 찍기");
        _pickStart.Click += (_, _) =>
        {
            if (!BeginPick(TerrainPickKind.Start))
                _status.Text = "지형 보기를 먼저 누르세요.";
        };
        _pickEnd.Click += (_, _) =>
        {
            if (!BeginPick(TerrainPickKind.End))
                _status.Text = "지형 보기를 먼저 누르세요.";
        };

        _showAll = new Button
        {
            Text = "모두 보기",
            AutoSize = true,
            MinimumSize = new Size(120, 32),
            Margin = new Padding(0, 10, 0, 0)
        };
        _showAll.Click += (_, _) => ShowRoutes(null);

        var fit = new Button
        {
            Text = "전체 보기",
            AutoSize = true,
            MinimumSize = new Size(120, 32),
            Margin = new Padding(0, 6, 0, 0)
        };
        fit.Click += (_, _) => _map.Fit();

        var sideTitle = new Label
        {
            AutoSize = true,
            Text = "고도",
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6)
        };
        var side = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12, 8, 8, 8),
            BackColor = Color.FromArgb(245, 240, 238)
        };
        side.Controls.Add(_pickStart);
        side.Controls.Add(_pickEnd);
        side.Controls.Add(sideTitle);
        side.Controls.Add(_maxLabel);
        side.Controls.Add(_ramp);
        side.Controls.Add(_minLabel);
        side.Controls.Add(_markers);
        side.Controls.Add(_routes);
        side.Controls.Add(_showAll);
        side.Controls.Add(fit);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 196));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(_map, 0, 0);
        body.Controls.Add(side, 1, 0);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.Controls.Add(_header, 0, 0);
        root.Controls.Add(body, 0, 1);
        root.Controls.Add(_status, 0, 2);
        Controls.Add(root);
    }

    public void Clear()
    {
        _grid = null;
        _profiles = new Dictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>>();
        _start = null;
        _end = null;
        _overviewBitmap?.Dispose();
        _overviewBitmap = null;
        _header.Text = "지형 보기를 누르면 고도가 여기에 표시됩니다.";
        _maxLabel.Text = "";
        _minLabel.Text = "";
        _routes.Controls.Clear();
        _routeChecks.Clear();
        _status.Text = TerrainMapControl.Hint;
        _map.Clear();
        CancelPick();
        _emphasized = null;
        _ramp.Invalidate();
    }

    public event EventHandler<TerrainPickKind?>? PickModeChanged;

    public event EventHandler<TerrainPointPick>? PointPicked;

    public bool BeginPick(TerrainPickKind kind)
    {
        if (_grid == null)
            return false;

        _pick = _pick == kind ? null : kind;
        UpdatePickButtons();
        _status.Text = _pick switch
        {
            TerrainPickKind.Start => "시점을 클릭하세요. Esc로 취소합니다.",
            TerrainPickKind.End => "종점을 클릭하세요. Esc로 취소합니다.",
            _ => TerrainMapControl.Hint
        };
        PickModeChanged?.Invoke(this, _pick);
        if (_pick != null)
            _map.Focus();
        return true;
    }

    public void ShowRoutes(RoadObjective? focus)
    {
        if (_grid == null)
            return;

        _emphasized = focus;
        if (focus is RoadObjective one)
        {
            if (_routeChecks.TryGetValue(one, out CheckBox? box))
                box.Checked = true;
        }
        else
        {
            foreach (CheckBox box in _routeChecks.Values)
                box.Checked = true;
        }

        _map.FitToRoutes(focus);
        _map.Invalidate();
    }

    public void SetInteractionEnabled(bool enabled)
    {
        _pickStart.Enabled = enabled;
        _pickEnd.Enabled = enabled;
        _showAll.Enabled = enabled;
        _routes.Enabled = enabled;
        if (!enabled)
            CancelPick();
    }

    internal RoadObjective? Emphasized => _emphasized;

    internal bool IsPicking => _pick != null;

    internal void AcceptMapClick(Point location)
    {
        if (_pick is not TerrainPickKind kind || _grid == null)
            return;
        if (!_map.TryScreenToWorld(location, out double x, out double y))
            return;

        double? z = _grid.TryGetCellZ(x, y, out double stored) ? stored : null;
        _pick = null;
        UpdatePickButtons();
        _status.Text = (kind == TerrainPickKind.Start ? "시점" : "종점")
            + "  X " + x.ToString("0.000", CultureInfo.CurrentCulture)
            + "    Y " + y.ToString("0.000", CultureInfo.CurrentCulture)
            + (z is double height
                ? "    고도 " + height.ToString("0.000", CultureInfo.CurrentCulture) + " m"
                : "    고도 없음");
        PointPicked?.Invoke(this, new TerrainPointPick(kind, x, y, z));
    }

    private void CancelPick()
    {
        if (_pick == null)
        {
            UpdatePickButtons();
            return;
        }

        _pick = null;
        UpdatePickButtons();
        _status.Text = TerrainMapControl.Hint;
        PickModeChanged?.Invoke(this, null);
    }

    private void UpdatePickButtons()
    {
        StylePickButton(_pickStart, _pick == TerrainPickKind.Start);
        StylePickButton(_pickEnd, _pick == TerrainPickKind.End);
    }

    private static void StylePickButton(Button button, bool active)
    {
        button.BackColor = active ? Color.FromArgb(198, 40, 40) : Color.White;
        button.ForeColor = active ? Color.White : Color.Black;
    }

    private static Button CreatePickButton(string text) => new()
    {
        Text = text,
        AutoSize = true,
        MinimumSize = new Size(120, 32),
        Margin = new Padding(0, 0, 0, 4),
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.White
    };

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape && _pick != null)
        {
            CancelPick();
            return true;
        }

        if (keyData is Keys.D0 or Keys.NumPad0)
        {
            _map.Fit();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _overviewBitmap?.Dispose();
            _overviewBitmap = null;
        }

        base.Dispose(disposing);
    }

    public void ShowContent(
        ElevationGrid grid,
        TerrainOverview overview,
        IReadOnlyDictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>> profiles,
        (double X, double Y)? start,
        (double X, double Y)? end)
    {
        bool sameGrid = ReferenceEquals(_grid, grid);
        _grid = grid;
        _overview = overview;
        _profiles = profiles;
        _start = start;
        _end = end;

        _header.Text = Path.GetFileName(grid.SourcePath)
            + "    칸 " + grid.CellSizeMeters.ToString("0.##", CultureInfo.CurrentCulture) + "m    "
            + grid.ElevationRule;
        _maxLabel.Text = overview.MaxZ.ToString("0.0", CultureInfo.CurrentCulture) + " m";
        _minLabel.Text = overview.MinZ.ToString("0.0", CultureInfo.CurrentCulture) + " m";
        _ramp.Invalidate();
        BindRoutes(profiles);

        if (!sameGrid || _overviewBitmap == null)
        {
            _overviewBitmap?.Dispose();
            double metersX = (grid.GridMaxX - grid.GridMinX) / overview.Width;
            double metersY = (grid.GridMaxY - grid.GridMinY) / overview.Height;
            _overviewBitmap = TerrainColor.Colorize(
                overview.Samples,
                overview.Width,
                overview.Height,
                overview.MinZ,
                overview.MaxZ,
                metersX,
                metersY);
        }

        _map.SetContent(grid, overview, _overviewBitmap, sameGrid);
        _map.Invalidate();
    }

    private void BindRoutes(
        IReadOnlyDictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>> profiles)
    {
        var previous = _routeChecks.ToDictionary(pair => pair.Key, pair => pair.Value.Checked);
        _routes.Controls.Clear();
        _routeChecks.Clear();

        foreach (RoadObjective objective in Enum.GetValues<RoadObjective>())
        {
            if (!profiles.TryGetValue(objective, out IReadOnlyList<RoadDesignVertex>? profile)
                || profile.Count < 2)
                continue;

            var row = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0, 1, 0, 1)
            };
            var box = new CheckBox
            {
                Text = RoadObjectiveStyle.Caption(objective),
                Checked = !previous.TryGetValue(objective, out bool wasChecked) || wasChecked,
                AutoSize = true,
                MaximumSize = new Size(108, 0),
                ForeColor = RoadObjectiveStyle.ColorOf(objective),
                Margin = new Padding(0, 4, 0, 0)
            };
            var show = new Button
            {
                Text = "보기",
                AutoSize = true,
                Margin = new Padding(2, 0, 0, 0),
                Padding = new Padding(4, 0, 4, 0)
            };
            RoadObjective chosen = objective;
            box.CheckedChanged += (_, _) => _map.Invalidate();
            show.Click += (_, _) => ShowRoutes(chosen);
            _routeChecks[objective] = box;
            row.Controls.Add(box);
            row.Controls.Add(show);
            _routes.Controls.Add(row);
        }

        if (_routeChecks.Count == 0)
        {
            _routes.Controls.Add(new Label
            {
                Text = "후보 노선이 없습니다.",
                AutoSize = true,
                MaximumSize = new Size(160, 0),
                ForeColor = Color.FromArgb(90, 90, 90),
                Margin = new Padding(0, 8, 0, 0)
            });
        }
    }

    private void DrawRamp(object? sender, PaintEventArgs e)
    {
        Rectangle bounds = _ramp.ClientRectangle;
        if (bounds.Width < 2 || bounds.Height < 2 || _grid == null)
            return;

        int width = Math.Min(28, bounds.Width);
        for (int y = 0; y < bounds.Height; y++)
        {
            float t = 1f - y / (float)Math.Max(1, bounds.Height - 1);
            using var pen = new Pen(TerrainColor.Ramp(t));
            e.Graphics.DrawLine(pen, 0, y, width, y);
        }
    }

    private void UpdateStatusHint()
    {
        if (!_map.PointerInside)
            _status.Text = TerrainMapControl.Hint;
    }

    internal bool ShowsMarkers => _markers.Checked;

    internal bool Shows(RoadObjective objective) =>
        _routeChecks.TryGetValue(objective, out CheckBox? box) && box.Checked;

    internal (double X, double Y)? Start => _start;

    internal (double X, double Y)? End => _end;

    internal IReadOnlyDictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>> Profiles => _profiles;
}
