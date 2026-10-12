using find_forestLoad.Las;

namespace find_forestLoad;

internal sealed partial class CadRoutePreviewDialog : Form
{
    public CadRoutePreviewDialog()
    {
        InitializeComponent();
    }

    public RoadObjective? SelectedObjective { get; private set; }

    public bool ShowAll { get; private set; }

    public void BindCandidates(
        IReadOnlyDictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>> profiles)
    {
        Bind(buttonSafety, RoadObjective.Safety, "안정성", profiles);
        Bind(buttonCost, RoadObjective.Cost, "비용 추정", profiles);
        Bind(buttonDistance, RoadObjective.Distance, "거리 후보 / 부드러운 우회 초안", profiles);
        Bind(buttonTime, RoadObjective.ConstructionTime, "공사 기간 추정", profiles);
    }

    private static void Bind(
        Button button,
        RoadObjective objective,
        string caption,
        IReadOnlyDictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>> profiles)
    {
        if (!profiles.TryGetValue(objective, out IReadOnlyList<RoadDesignVertex>? profile))
        {
            button.Visible = false;
            return;
        }

        button.Visible = true;
        button.Text = $"{caption}  ·  꼭짓점 {profile.Count}개";
    }

    private void buttonSafety_Click(object? sender, EventArgs e) =>
        Choose(RoadObjective.Safety);

    private void buttonCost_Click(object? sender, EventArgs e) =>
        Choose(RoadObjective.Cost);

    private void buttonDistance_Click(object? sender, EventArgs e) =>
        Choose(RoadObjective.Distance);

    private void buttonTime_Click(object? sender, EventArgs e) =>
        Choose(RoadObjective.ConstructionTime);

    private void buttonShowAll_Click(object? sender, EventArgs e)
    {
        ShowAll = true;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void Choose(RoadObjective objective)
    {
        SelectedObjective = objective;
        DialogResult = DialogResult.OK;
        Close();
    }
}
