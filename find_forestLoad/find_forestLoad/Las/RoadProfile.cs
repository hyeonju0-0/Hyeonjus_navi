namespace find_forestLoad.Las;

public static class RoadProfile
{
    /// <summary>
    /// 경로의 누적 거리에 따라 시점과 종점 사이의 도로 높이를 배정한다.
    /// 결과는 초기 종단 계획이며, 절토·성토와 곡선 설계 검토가 필요하다.
    /// </summary>
    public static IReadOnlyList<RoadDesignVertex> Build(
        IReadOnlyList<RoadVertex> groundPath)
    {
        if (groundPath.Count == 0)
            throw new ArgumentException("경로에 점이 없습니다.", nameof(groundPath));

        var distances = new double[groundPath.Count];
        double totalLength = 0;

        for (int i = 1; i < groundPath.Count; i++)
        {
            double dx = groundPath[i].X - groundPath[i - 1].X;
            double dy = groundPath[i].Y - groundPath[i - 1].Y;

            totalLength += Math.Sqrt(dx * dx + dy * dy);
            distances[i] = totalLength;
        }

        double startZ = groundPath[0].Z;
        double endZ = groundPath[^1].Z;
        var result = new List<RoadDesignVertex>(groundPath.Count);

        for (int i = 0; i < groundPath.Count; i++)
        {
            RoadVertex point = groundPath[i];

            double fraction =
                totalLength > 0 ? distances[i] / totalLength : 0;

            double roadZ = startZ + (endZ - startZ) * fraction;

            result.Add(new RoadDesignVertex(
                point.X,
                point.Y,
                GroundZ: point.Z,
                RoadZ: roadZ));
        }

        return result;
    }

    public static double GetMaximumGradePercent(
        IReadOnlyList<RoadDesignVertex> road)
    {
        double maximum = 0;

        for (int i = 1; i < road.Count; i++)
        {
            double dx = road[i].X - road[i - 1].X;
            double dy = road[i].Y - road[i - 1].Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= 0)
                continue;

            double grade =
                Math.Abs(road[i].RoadZ - road[i - 1].RoadZ)
                / distance * 100;

            maximum = Math.Max(maximum, grade);
        }

        return maximum;
    }

    public static bool TryEvaluate(
    IReadOnlyList<RoadVertex> groundPath,
    double maximumGradePercent,
    double maximumCutFillMeters,
    out IReadOnlyList<RoadDesignVertex> designedRoad,
    out string reason)
    {
        designedRoad = Array.Empty<RoadDesignVertex>();

        if (groundPath.Count < 2)
        {
            reason = "경로에 점이 두 개 이상 필요합니다.";
            return false;
        }

        if (maximumGradePercent <= 0 || maximumCutFillMeters < 0)
        {
            reason = "경사와 절토·성토 한도를 확인하세요.";
            return false;
        }

        IReadOnlyList<RoadDesignVertex> result = Build(groundPath);
        double grade = GetMaximumGradePercent(result);

        if (grade > maximumGradePercent + 0.0001)
        {
            reason = $"설계 도로 경사 {grade:0.0}%가 기준 " +
                     $"{maximumGradePercent:0.0}%를 넘습니다.";
            return false;
        }

        double largestCut = result.Max(point => point.CutHeight);
        double largestFill = result.Max(point => point.FillHeight);

        if (largestCut > maximumCutFillMeters ||
            largestFill > maximumCutFillMeters)
        {
            reason = $"절토 최대 {largestCut:0.0}m, 성토 최대 " +
                     $"{largestFill:0.0}m로 설정한 한도를 넘습니다.";
            return false;
        }

        designedRoad = result;
        reason = "기본 경사 및 절토·성토 높이 검사를 통과했습니다.";
        return true;
    }

    public static bool TryAcceptDesigned(
        IReadOnlyList<RoadDesignVertex> designed,
        double maximumGradePercent,
        double maximumCutFillMeters,
        out string reason)
    {
        if (designed.Count < 2)
        {
            reason = "경로에 점이 두 개 이상 필요합니다.";
            return false;
        }

        if (maximumGradePercent <= 0 || maximumCutFillMeters < 0)
        {
            reason = "경사와 절토·성토 한도를 확인하세요.";
            return false;
        }

        foreach (RoadDesignVertex point in designed)
        {
            if (!double.IsFinite(point.RoadZ) || !double.IsFinite(point.GroundZ))
            {
                reason = "경로에 잘못된 높이가 있습니다.";
                return false;
            }

            if (point.CutHeight > maximumCutFillMeters + 0.02 ||
                point.FillHeight > maximumCutFillMeters + 0.02)
            {
                reason = $"절토 최대 {point.CutHeight:0.00}m, 성토 최대 " +
                         $"{point.FillHeight:0.00}m로 한도를 넘습니다.";
                return false;
            }
        }

        for (int i = 1; i < designed.Count; i++)
        {
            double dx = designed[i].X - designed[i - 1].X;
            double dy = designed[i].Y - designed[i - 1].Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double rise = Math.Abs(designed[i].RoadZ - designed[i - 1].RoadZ);

            if (distance <= 0.05)
            {
                if (rise > 0.02)
                {
                    reason = "같은 위치에 높이가 다른 점이 있습니다.";
                    return false;
                }

                continue;
            }

            double grade = rise / distance * 100;
            if (grade > maximumGradePercent + 0.05)
            {
                reason = $"설계 도로 경사 {grade:0.0}%가 기준 " +
                         $"{maximumGradePercent:0.0}%를 넘습니다.";
                return false;
            }
        }

        reason = "경사와 절토·성토 높이 검사를 통과했습니다.";
        return true;
    }

}