namespace find_forestLoad.Las;

public sealed partial class ElevationGrid
{
    public DesignedRoadSearchResult FindSmoothDetourRoad(
        double startX,
        double startY,
        double endX,
        double endY,
        double snapRadiusMeters,
        double maximumGradePercent,
        double maximumCutFillMeters,
        double? minimumCurveRadiusMeters,
        CancellationToken cancellationToken = default)
    {
        if (maximumGradePercent <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(maximumGradePercent));

        if (!TrySnapCell(startX, startY, snapRadiusMeters,
                out int startIx, out int startIy))
            return new(null, "시점 주변에 고도가 없습니다.");

        if (!TrySnapCell(endX, endY, snapRadiusMeters,
                out int endIx, out int endIy))
            return new(null, "종점 주변에 고도가 없습니다.");

        double snappedStartX = CenterX(startIx);
        double snappedStartY = CenterY(startIy);
        double snappedEndX = CenterX(endIx);
        double snappedEndY = CenterY(endIy);

        double startZ = _cells[Key(startIx, startIy)];
        double endZ = _cells[Key(endIx, endIy)];

        double minimumLength =
            Math.Abs(endZ - startZ) /
            (maximumGradePercent / 100.0);

        int longEnoughCount = 0;
        int curvePassedCount = 0;
        int groundPassedCount = 0;
        int acceptedCount = 0;
        string lastProfileReason = "";
        IReadOnlyList<RoadDesignVertex>? best = null;
        double bestScore = double.PositiveInfinity;
        var trials = new List<DetourTrial>();

        // 짧은 우회부터 양쪽을 모두 시험하고, 통과한 곡선 중 비용이 가장 낮은 것을 고른다.
        for (double offset = 0; offset <= 400; offset += 10)
        {
            foreach (double signedOffset in offset == 0
                         ? new[] { 0.0 }
                         : new[] { offset, -offset })
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyList<(double X, double Y)> plan =
                    RoadGeometry.BuildSmoothDetour(
                        snappedStartX, snappedStartY,
                        snappedEndX, snappedEndY,
                        signedOffset);

                double length = RoadGeometry.GetPlanLength(plan);
                double measuredRadius = RoadGeometry.GetMinimumCurveRadius(plan);
                double? curveRadius = double.IsFinite(measuredRadius) ? measuredRadius : null;

                if (length + 0.001 < minimumLength)
                {
                    trials.Add(new DetourTrial(
                        signedOffset,
                        false,
                        "평면 길이가 고도 차이에 필요한 최소 길이보다 짧습니다.",
                        null,
                        length,
                        curveRadius));
                    continue;
                }

                longEnoughCount++;

                if (minimumCurveRadiusMeters is double radius &&
                    measuredRadius + 0.001 < radius)
                {
                    trials.Add(new DetourTrial(
                        signedOffset,
                        false,
                        "곡선 반지름이 임도 최소 반지름보다 작습니다.",
                        null,
                        length,
                        curveRadius));
                    continue;
                }

                curvePassedCount++;

                var groundPath = new List<RoadVertex>(plan.Count);
                bool missingGround = false;

                for (int i = 0; i < plan.Count; i++)
                {
                    if (i == 0)
                    {
                        groundPath.Add(new RoadVertex(
                            plan[i].X, plan[i].Y, startZ));
                        continue;
                    }

                    if (i == plan.Count - 1)
                    {
                        groundPath.Add(new RoadVertex(
                            plan[i].X, plan[i].Y, endZ));
                        continue;
                    }

                    if (!TrySampleGroundAt(plan[i].X, plan[i].Y, out float groundZ))
                    {
                        missingGround = true;
                        break;
                    }

                    groundPath.Add(new RoadVertex(
                        plan[i].X,
                        plan[i].Y,
                        groundZ));
                }

                if (missingGround)
                {
                    trials.Add(new DetourTrial(
                        signedOffset,
                        false,
                        "곡선 위에서 지면 높이를 읽지 못한 점이 있습니다.",
                        null,
                        length,
                        curveRadius));
                    continue;
                }

                groundPassedCount++;

                if (!RoadProfile.TryEvaluate(
                        groundPath,
                        maximumGradePercent,
                        maximumCutFillMeters,
                        out IReadOnlyList<RoadDesignVertex> designed,
                        out string profileReason))
                {
                    lastProfileReason = profileReason;
                    trials.Add(new DetourTrial(
                        signedOffset,
                        false,
                        profileReason,
                        null,
                        length,
                        curveRadius));
                    continue;
                }

                acceptedCount++;
                double score = DetourCost(designed);
                trials.Add(new DetourTrial(
                    signedOffset,
                    true,
                    null,
                    score,
                    length,
                    curveRadius));
                if (score < bestScore)
                {
                    bestScore = score;
                    best = designed;
                }
            }
        }

        IReadOnlyList<SmoothDetourSample> samples = RankDetourTrials(trials);

        if (best != null)
            return new(best, null) { DetourSamples = samples };

        return new(null,
            $"부드러운 우회 시험: 필요한 길이 충족 {longEnoughCount}개, " +
            $"곡률 검사 통과 {curvePassedCount}개, " +
            $"LAS 고도 연결 {groundPassedCount}개, " +
            $"종단 검사 통과 {acceptedCount}개. " +
            $"종단 검사 마지막 사유: {lastProfileReason}")
        { DetourSamples = samples };
    }

    private readonly record struct DetourTrial(
        double OffsetMeters,
        bool Accepted,
        string? Rejection,
        double? CostScore,
        double LengthMeters,
        double? CurveRadiusMeters);

    private static IReadOnlyList<SmoothDetourSample> RankDetourTrials(
        List<DetourTrial> trials)
    {
        var rankByIndex = new Dictionary<int, int>();
        int rank = 1;
        foreach (int index in trials
                     .Select((trial, index) => (trial, index))
                     .Where(pair => pair.trial.Accepted)
                     .OrderBy(pair => pair.trial.CostScore)
                     .ThenBy(pair => pair.index)
                     .Select(pair => pair.index))
        {
            rankByIndex[index] = rank;
            rank++;
        }

        var samples = new SmoothDetourSample[trials.Count];
        for (int i = 0; i < trials.Count; i++)
        {
            DetourTrial trial = trials[i];
            samples[i] = new SmoothDetourSample(
                trial.OffsetMeters,
                trial.Accepted,
                trial.Accepted ? null : trial.Rejection,
                trial.Accepted ? trial.CostScore : null,
                rankByIndex.TryGetValue(i, out int sampleRank) ? sampleRank : null,
                trial.LengthMeters,
                trial.CurveRadiusMeters);
        }

        return samples;
    }

    // A* 비용 목표와 같다. 길이에 그 구간의 절토·성토 높이를 더한 값이 작을수록 낫다.
    private static double DetourCost(IReadOnlyList<RoadDesignVertex> road)
    {
        double score = 0;

        for (int i = 1; i < road.Count; i++)
        {
            double dx = road[i].X - road[i - 1].X;
            double dy = road[i].Y - road[i - 1].Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double cutFill = Math.Max(road[i].CutHeight, road[i].FillHeight);
            score += distance * (1.0 + 0.4 * cutFill);
        }

        return score;
    }
}