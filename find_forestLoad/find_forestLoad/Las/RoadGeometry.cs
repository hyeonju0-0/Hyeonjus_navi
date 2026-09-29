namespace find_forestLoad.Las;

public readonly record struct RoadCorner(
    int VertexIndex,
    double TurnDegrees,
    double BeforeMeters,
    double AfterMeters);

public readonly record struct CurveSpaceIssue(
    int FromVertex,
    int ToVertex,
    double AvailableMeters,
    double RequiredMeters);

public static class RoadGeometry
{

    public static bool HasRepeatedPlanPoint(
    IReadOnlyList<RoadDesignVertex> points)
    {
        var visited = new HashSet<(double X, double Y)>();

        foreach (RoadDesignVertex point in points)
        {
            if (!visited.Add((point.X, point.Y)))
                return true;
        }

        return false;
    }

    public static IReadOnlyList<RoadCorner> FindSharpCorners(
        IReadOnlyList<RoadDesignVertex> points)
    {
        var corners = new List<RoadCorner>();

        for (int i = 1; i < points.Count - 1; i++)
        {
            double ax = points[i].X - points[i - 1].X;
            double ay = points[i].Y - points[i - 1].Y;
            double bx = points[i + 1].X - points[i].X;
            double by = points[i + 1].Y - points[i].Y;

            double before = Math.Sqrt(ax * ax + ay * ay);
            double after = Math.Sqrt(bx * bx + by * by);

            if (before < 0.0001 || after < 0.0001)
                continue;

            double cosine =
                (ax * bx + ay * by) / (before * after);

            double turnDegrees =
                Math.Acos(Math.Clamp(cosine, -1, 1))
                * 180 / Math.PI;

            if (turnDegrees > 25)
            {
                corners.Add(new RoadCorner(
                    i, turnDegrees, before, after));
            }
        }

        return corners;
    }

    public static IReadOnlyList<CurveSpaceIssue> FindCurveSpaceIssues(
    IReadOnlyList<RoadDesignVertex> points,
    double radiusMeters)
    {
        if (radiusMeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(radiusMeters));

        IReadOnlyList<RoadCorner> corners =
            FindSharpCorners(points);

        var issues = new List<CurveSpaceIssue>();

        if (corners.Count == 0)
            return issues;

        var accumulated = new double[points.Count];

        for (int i = 1; i < points.Count; i++)
        {
            double dx = points[i].X - points[i - 1].X;
            double dy = points[i].Y - points[i - 1].Y;

            accumulated[i] = accumulated[i - 1] +
                Math.Sqrt(dx * dx + dy * dy);
        }

        double TangentLength(RoadCorner corner) =>
            radiusMeters *
            Math.Tan(corner.TurnDegrees * Math.PI / 360.0);

        // 시점에서 첫 곡선까지 필요한 길이
        RoadCorner first = corners[0];
        CheckSpan(
            0,
            first.VertexIndex,
            0,
            TangentLength(first));

        // 연속된 두 곡선 사이에는 양쪽 곡선의 길이가 모두 필요
        for (int i = 1; i < corners.Count; i++)
        {
            RoadCorner previous = corners[i - 1];
            RoadCorner current = corners[i];

            CheckSpan(
                previous.VertexIndex,
                current.VertexIndex,
                TangentLength(previous),
                TangentLength(current));
        }

        // 마지막 곡선에서 종점까지 필요한 길이
        RoadCorner last = corners[^1];
        CheckSpan(
            last.VertexIndex,
            points.Count - 1,
            TangentLength(last),
            0);

        return issues;

        void CheckSpan(
            int fromIndex,
            int toIndex,
            double firstNeed,
            double secondNeed)
        {
            double available =
                accumulated[toIndex] - accumulated[fromIndex];

            double required =
                firstNeed + secondNeed;

            if (available + 0.0001 < required)
            {
                issues.Add(new CurveSpaceIssue(
                    fromIndex,
                    toIndex,
                    available,
                    required));
            }
        }
    }

    public static IReadOnlyList<(double X, double Y)> BuildSmoothDetour(
    double startX,
    double startY,
    double endX,
    double endY,
    double sideOffsetMeters,
    double sampleStepMeters = 0.5)
    {
        if (sampleStepMeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleStepMeters));

        double dx = endX - startX;
        double dy = endY - startY;
        double straightLength = Math.Sqrt(dx * dx + dy * dy);

        if (straightLength < 0.001)
            return [(startX, startY)];

        // 시점→종점 방향에 수직인 방향으로 중간 제어점을 이동한다.
        double normalX = -dy / straightLength;
        double normalY = dx / straightLength;

        double controlX = (startX + endX) / 2.0
                        + normalX * sideOffsetMeters;
        double controlY = (startY + endY) / 2.0
                        + normalY * sideOffsetMeters;

        double firstLength = Math.Sqrt(
            Math.Pow(controlX - startX, 2) +
            Math.Pow(controlY - startY, 2));

        double secondLength = Math.Sqrt(
            Math.Pow(endX - controlX, 2) +
            Math.Pow(endY - controlY, 2));

        int intervals = Math.Max(
            2,
            (int)Math.Ceiling(
                (firstLength + secondLength) / sampleStepMeters));

        var points = new List<(double X, double Y)>(intervals + 1);

        for (int i = 0; i <= intervals; i++)
        {
            double t = (double)i / intervals;
            double a = 1.0 - t;

            double x = a * a * startX
                     + 2.0 * a * t * controlX
                     + t * t * endX;

            double y = a * a * startY
                     + 2.0 * a * t * controlY
                     + t * t * endY;

            points.Add((x, y));
        }

        return points;
    }

    public static double GetPlanLength(
    IReadOnlyList<(double X, double Y)> points)
    {
        double length = 0;

        for (int i = 1; i < points.Count; i++)
        {
            double dx = points[i].X - points[i - 1].X;
            double dy = points[i].Y - points[i - 1].Y;
            length += Math.Sqrt(dx * dx + dy * dy);
        }

        return length;
    }

    public static double GetMinimumCurveRadius(
        IReadOnlyList<(double X, double Y)> points)
    {
        double minimum = double.PositiveInfinity;

        for (int i = 1; i < points.Count - 1; i++)
        {
            double ax = points[i].X - points[i - 1].X;
            double ay = points[i].Y - points[i - 1].Y;
            double bx = points[i + 1].X - points[i].X;
            double by = points[i + 1].Y - points[i].Y;

            double before = Math.Sqrt(ax * ax + ay * ay);
            double after = Math.Sqrt(bx * bx + by * by);
            double chord = Math.Sqrt(
                Math.Pow(points[i + 1].X - points[i - 1].X, 2) +
                Math.Pow(points[i + 1].Y - points[i - 1].Y, 2));

            double cross = Math.Abs(ax * by - ay * bx);

            if (before < 0.000001 ||
                after < 0.000001 ||
                cross < 0.000001)
                continue;

            double radius =
                before * after * chord / (2.0 * cross);

            minimum = Math.Min(minimum, radius);
        }

        return minimum;
    }

}