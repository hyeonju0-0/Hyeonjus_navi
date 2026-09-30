namespace find_forestLoad.Las;

public enum RoadKind
{
    Arterial,       // 간선임도
    Firefighting,   // 산불진화임도
    Work            // 작업임도
}

public enum TerrainKind
{
    Normal,         // 일반지형
    Special         // 특수지형
}

public sealed record RoadStandard(
    RoadKind Kind,
    TerrainKind Terrain,
    int DesignSpeedKmh,
    double MaximumGradePercent,
    double EffectiveWidthMeters,
    double ReverseCurveWidthMeters)
{
    // 노선 탐색을 위한 임시 한도이며 법정 절토·성토 기준값이 아니다.
    public const double PreliminaryCutFillLimitMeters = 3.0;

    public string KindName => Kind switch
    {
        RoadKind.Arterial => "간선임도",
        RoadKind.Firefighting => "산불진화임도",
        RoadKind.Work => "작업임도",
        _ => throw new ArgumentOutOfRangeException(nameof(Kind))
    };

    // 간선·산불진화임도의 곡선부 중심선 최소 반지름.
    // 작업임도에는 이 표를 그대로 적용하지 않는다.
    public double? MinimumCenterlineCurveRadiusMeters =>
        Kind == RoadKind.Work
            ? null
            : (DesignSpeedKmh, Terrain) switch
            {
                (40, TerrainKind.Normal) => 60,
                (30, TerrainKind.Normal) => 30,
                (20, TerrainKind.Normal) => 15,

                (40, TerrainKind.Special) => 40,
                (30, TerrainKind.Special) => 20,
                (20, TerrainKind.Special) => 12,

                _ => throw new InvalidOperationException(
                    "설계속도 또는 지형 설정이 올바르지 않습니다.")
            };

    public static RoadStandard Create(
        RoadKind kind,
        TerrainKind terrain,
        int designSpeedKmh)
    {
        if (kind == RoadKind.Work)
        {
            if (designSpeedKmh != 20)
                throw new ArgumentOutOfRangeException(nameof(designSpeedKmh));

            return new RoadStandard(
                kind, terrain, designSpeedKmh,
                MaximumGradePercent: 20,
                EffectiveWidthMeters: 2.5,
                ReverseCurveWidthMeters: 6);
        }

        double maximumGrade = (designSpeedKmh, terrain) switch
        {
            (40, TerrainKind.Normal) => 7,
            (30, TerrainKind.Normal) => 8,
            (20, TerrainKind.Normal) => 9,

            (40, TerrainKind.Special) => 10,
            (30, TerrainKind.Special) => 12,
            (20, TerrainKind.Special) => 14,

            _ => throw new ArgumentOutOfRangeException(nameof(designSpeedKmh))
        };

        return new RoadStandard(
            kind, terrain, designSpeedKmh,
            MaximumGradePercent: maximumGrade,
            EffectiveWidthMeters: kind == RoadKind.Firefighting ? 3.5 : 3,
            ReverseCurveWidthMeters: kind == RoadKind.Firefighting ? 8 : 6);
    }

    public RoadStandard WithMaximumGradePercent(double maximumGradePercent)
    {
        if (!double.IsFinite(maximumGradePercent) ||
            maximumGradePercent <= 0 ||
            maximumGradePercent > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumGradePercent),
                "허용 경사는 0보다 크고 100% 이하여야 합니다.");
        }

        return this with { MaximumGradePercent = maximumGradePercent };
    }
}