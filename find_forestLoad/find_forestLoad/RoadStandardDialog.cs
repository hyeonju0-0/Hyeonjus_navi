using find_forestLoad.Las;

namespace find_forestLoad;

internal sealed class RoadStandardDialog : Form
{
    private readonly ComboBox roadKind = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Dock = DockStyle.Fill
    };

    private readonly ComboBox terrainKind = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Dock = DockStyle.Fill
    };

    private readonly ComboBox designSpeed = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Dock = DockStyle.Fill
    };

    private readonly NumericUpDown maximumGrade = new()
    {
        DecimalPlaces = 1,
        Increment = 0.5m,
        Minimum = 0.1m,
        Maximum = 100m,
        Value = 9m,
        Dock = DockStyle.Fill,
        TextAlign = HorizontalAlignment.Right,
        ThousandsSeparator = false
    };

    private readonly Label summary = new()
    {
        Dock = DockStyle.Fill
    };

    private decimal? _lockedGrade;
    private bool _syncingGrade;

    public RoadStandard SelectedStandard =>
        BaselineStandard().WithMaximumGradePercent((double)maximumGrade.Value);

    public RoadStandardDialog(double? initialMaximumGradePercent = null)
    {
        if (initialMaximumGradePercent is double grade)
        {
            if (!double.IsFinite(grade) || grade <= 0 || grade > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialMaximumGradePercent),
                    "허용 경사는 0보다 크고 100% 이하여야 합니다.");
            }

            _lockedGrade = (decimal)grade;
        }

        Text = "임도 종류와 적용 기준";
        ClientSize = new Size(560, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 6
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        var gradeRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        gradeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        gradeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));

        var resetGradeButton = new Button
        {
            Text = "기준값",
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 0, 0, 0)
        };
        resetGradeButton.Click += (_, _) =>
        {
            _lockedGrade = null;
            UpdateSummary();
        };

        gradeRow.Controls.Add(maximumGrade, 0, 0);
        gradeRow.Controls.Add(resetGradeButton, 1, 0);

        AddRow(layout, 0, "임도 종류", roadKind);
        AddRow(layout, 1, "지형", terrainKind);
        AddRow(layout, 2, "설계속도", designSpeed);
        AddRow(layout, 3, "허용 경사(%)", gradeRow);

        layout.Controls.Add(summary, 0, 4);
        layout.SetColumnSpan(summary, 2);

        var confirmButton = new Button
        {
            Text = "이 기준으로 탐색",
            Width = 155,
            Dock = DockStyle.Right,
            DialogResult = DialogResult.OK
        };

        layout.Controls.Add(confirmButton, 1, 5);
        Controls.Add(layout);
        AcceptButton = confirmButton;

        roadKind.Items.AddRange(
            new object[] { "간선임도", "산불진화임도", "작업임도" });

        terrainKind.Items.AddRange(
            new object[] { "일반지형", "특수지형" });

        maximumGrade.ValueChanged += (_, _) =>
        {
            if (_syncingGrade)
                return;

            _lockedGrade = maximumGrade.Value;
            UpdateSummary();
        };

        roadKind.SelectedIndexChanged += (_, _) => UpdateSpeed();
        terrainKind.SelectedIndexChanged += (_, _) => UpdateSummary();
        designSpeed.SelectedIndexChanged += (_, _) => UpdateSummary();

        terrainKind.SelectedIndex = 0;
        roadKind.SelectedIndex = 0;
    }

    private static void AddRow(
        TableLayoutPanel layout,
        int row,
        string caption,
        Control input)
    {
        layout.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, row);

        layout.Controls.Add(input, 1, row);
    }

    private RoadStandard BaselineStandard() => RoadStandard.Create(
        (RoadKind)roadKind.SelectedIndex,
        (TerrainKind)terrainKind.SelectedIndex,
        int.Parse(designSpeed.SelectedItem!.ToString()!));

    private void SetGrade(decimal value)
    {
        decimal clamped = Math.Clamp(value, maximumGrade.Minimum, maximumGrade.Maximum);
        if (maximumGrade.Value == clamped)
            return;

        _syncingGrade = true;
        maximumGrade.Value = clamped;
        _syncingGrade = false;
    }

    private void UpdateSpeed()
    {
        designSpeed.Items.Clear();

        if (roadKind.SelectedIndex == (int)RoadKind.Work)
            designSpeed.Items.Add("20");
        else
            designSpeed.Items.AddRange(new object[] { "20", "30", "40" });

        designSpeed.SelectedIndex = 0;
    }

    private void UpdateSummary()
    {
        if (roadKind.SelectedIndex < 0 ||
            terrainKind.SelectedIndex < 0 ||
            designSpeed.SelectedIndex < 0)
            return;

        RoadStandard baseline = BaselineStandard();
        decimal tableGrade = (decimal)baseline.MaximumGradePercent;
        decimal applied = _lockedGrade ?? tableGrade;
        SetGrade(applied);

        bool matchesTable = Math.Abs((double)applied - baseline.MaximumGradePercent) < 0.05;
        string gradeText = matchesTable
            ? $"적용 종단경사 한도: {applied:0.#}% (기준표)"
            : $"적용 종단경사 한도: {applied:0.#}% (직접 입력, 기준표 {tableGrade:0.#}%)";

        summary.Text =
            gradeText + "\n" +
            $"유효너비 기준: {baseline.EffectiveWidthMeters:0.#}m\n" +
            $"배향곡선지 너비: {baseline.ReverseCurveWidthMeters:0.#}m 이상\n\n" +
            "경사 칸에 값을 넣으면 그 한도로 탐색합니다. 기준값은 표 값으로 되돌립니다.\n" +
            "현재 A*는 경사만 탐색에 반영합니다.\n" +
            "도로 폭·곡선·배수는 다음 단계에서 검토합니다.";
    }
}
