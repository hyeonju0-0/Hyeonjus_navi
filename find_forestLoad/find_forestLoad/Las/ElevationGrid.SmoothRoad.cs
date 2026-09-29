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
        string lastProfileReason = "";

        // 짧은 우회부터 양쪽 방향을 번갈아 시험한다.
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

                if (length + 0.001 < minimumLength)
                    continue;

                longEnoughCount++;

                if (minimumCurveRadiusMeters is double radius &&
                    RoadGeometry.GetMinimumCurveRadius(plan) + 0.001 < radius)
                    continue;

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
                    continue;

                groundPassedCount++;

                if (RoadProfile.TryEvaluate(
                        groundPath,
                        maximumGradePercent,
                        maximumCutFillMeters,
                        out IReadOnlyList<RoadDesignVertex> designed,
                        out string profileReason))
                {
                    return new(designed, null);
                }

                lastProfileReason = profileReason;
            }
        }

        return new(null,
            $"부드러운 우회 시험: 필요한 길이 충족 {longEnoughCount}개, " +
            $"곡률 검사 통과 {curvePassedCount}개, " +
            $"LAS 고도 연결 {groundPassedCount}개. " +
            $"종단 검사 마지막 사유: {lastProfileReason}");
    }
}