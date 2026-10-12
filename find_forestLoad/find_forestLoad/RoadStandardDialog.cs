using find_forestLoad.Las;

namespace find_forestLoad;

internal sealed partial class RoadStandardDialog : Form
{
    private decimal? _lockedGrade;
    private bool _syncingGrade;

    public RoadStandard SelectedStandard =>
        BaselineStandard().WithMaximumGradePercent((double)maximumGrade.Value);

    public RoadStandardDialog()
    {
        InitializeComponent();

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
        resetGradeButton.Click += (_, _) =>
        {
            _lockedGrade = null;
            UpdateSummary();
        };

        if (terrainKind.SelectedIndex < 0)
            terrainKind.SelectedIndex = 0;

        if (roadKind.SelectedIndex < 0)
            roadKind.SelectedIndex = 0;
        else
            UpdateSpeed();
    }

    public RoadStandardDialog(double? initialMaximumGradePercent)
        : this()
    {
        if (initialMaximumGradePercent is not double grade)
            return;

        if (!double.IsFinite(grade) || grade <= 0 || grade > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialMaximumGradePercent),
                "허용 경사는 0보다 크고 100% 이하여야 합니다.");
        }

        _lockedGrade = (decimal)grade;
        UpdateSummary();
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
