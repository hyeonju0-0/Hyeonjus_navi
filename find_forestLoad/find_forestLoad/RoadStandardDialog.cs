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

    private readonly Label summary = new()
    {
        Dock = DockStyle.Fill
    };

    public RoadStandard SelectedStandard => RoadStandard.Create(
        (RoadKind)roadKind.SelectedIndex,
        (TerrainKind)terrainKind.SelectedIndex,
        int.Parse(designSpeed.SelectedItem!.ToString()!)
    );

    public RoadStandardDialog()
    {
        Text = "임도 종류와 적용 기준";
        ClientSize = new Size(520, 310);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 5
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        AddRow(layout, 0, "임도 종류", roadKind);
        AddRow(layout, 1, "지형", terrainKind);
        AddRow(layout, 2, "설계속도", designSpeed);

        layout.Controls.Add(summary, 0, 3);
        layout.SetColumnSpan(summary, 2);

        var confirmButton = new Button
        {
            Text = "이 기준으로 탐색",
            Width = 155,
            Dock = DockStyle.Right,
            DialogResult = DialogResult.OK
        };

        layout.Controls.Add(confirmButton, 1, 4);
        Controls.Add(layout);
        AcceptButton = confirmButton;

        roadKind.Items.AddRange(
            new object[] { "간선임도", "산불진화임도", "작업임도" });

        terrainKind.Items.AddRange(
            new object[] { "일반지형", "특수지형" });

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

        RoadStandard standard = SelectedStandard;

        summary.Text =
            $"기본 종단경사 한도: {standard.MaximumGradePercent:0.#}%\n" +
            $"유효너비 기준: {standard.EffectiveWidthMeters:0.#}m\n" +
            $"배향곡선지 너비: {standard.ReverseCurveWidthMeters:0.#}m 이상\n\n" +
            "현재 A*는 경사만 탐색에 반영합니다.\n" +
            "도로 폭·곡선·배수는 다음 단계에서 검토합니다.";
    }
}