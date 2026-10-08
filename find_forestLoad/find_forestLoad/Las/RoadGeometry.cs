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
    public const double StationIntervalMeters = 20;
    public const double BendTurnDegrees = 25;

    public static IReadOnlyList<RoadDesignVertex> SampleEvery(
        IReadOnlyList<RoadDesignVertex> points,
        double intervalMeters = StationIntervalMeters)
    {
        if (points.Count <= 1)
            return points;

        if (intervalMeters <= 0 || !double.IsFinite(intervalMeters))
            throw new ArgumentOutOfRangeException(nameof(intervalMeters));

        var sampled = new List<RoadDesignVertex> { points[0] };
        double next = intervalMeters;
        double traveled = 0;

        for (int i = 1; i < points.Count; i++)
        {
            RoadDesignVertex from = points[i - 1];
            RoadDesignVertex to = points[i];
            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double segment = Math.Sqrt(dx * dx + dy * dy);
            if (segment <= 1e-8)
                continue;

            while (traveled + segment >= next - 1e-6)
            {
                double t = Math.Clamp((next - traveled) / segment, 0, 1);
                sampled.Add(Interpolate(from, to, t));
                next += intervalMeters;
            }

            traveled += segment;
        }

        RoadDesignVertex end = points[^1];
        RoadDesignVertex last = sampled[^1];
        double remainX = end.X - last.X;
        double remainY = end.Y - last.Y;
        double remain = Math.Sqrt(remainX * remainX + remainY * remainY);
        if (remain <= 1e-6)
            return sampled;

        if (remain <= 1 && sampled.Count > 1)
            sampled[^1] = end;
        else
            sampled.Add(end);

        return sampled;
    }

    private static RoadDesignVertex Interpolate(
        RoadDesignVertex from,
        RoadDesignVertex to,
        double t)
    {
        return new RoadDesignVertex(
            from.X + (to.X - from.X) * t,
            from.Y + (to.Y - from.Y) * t,
            from.GroundZ + (to.GroundZ - from.GroundZ) * t,
            from.RoadZ + (to.RoadZ - from.RoadZ) * t);
    }

    public static IReadOnlyList<int> FindBendVertexIndices(
        IReadOnlyList<RoadVertex> vertices,
        double minimumTurnDegrees = BendTurnDegrees)
    {
        var bends = new List<int>();
        if (vertices.Count < 3 || minimumTurnDegrees < 0)
            return bends;

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            double ax = vertices[i].X - vertices[i - 1].X;
            double ay = vertices[i].Y - vertices[i - 1].Y;
            double bx = vertices[i + 1].X - vertices[i].X;
            double by = vertices[i + 1].Y - vertices[i].Y;
            double before = Math.Sqrt(ax * ax + ay * ay);
            double after = Math.Sqrt(bx * bx + by * by);
            if (before < 0.0001 || after < 0.0001)
                continue;

            double cosine = Math.Clamp((ax * bx + ay * by) / (before * after), -1, 1);
            double turnDegrees = Math.Acos(cosine) * 180 / Math.PI;
            if (turnDegrees >= minimumTurnDegrees)
                bends.Add(i);
        }

        return bends;
    }

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