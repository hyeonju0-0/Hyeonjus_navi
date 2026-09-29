namespace find_forestLoad.Las;

public sealed record DesignedRoadSearchResult(
    IReadOnlyList<RoadDesignVertex>? Vertices,
    string? Failure)
{
    public bool Succeeded => Vertices is { Count: > 0 };
}

public sealed partial class ElevationGrid
{
    private const int MaxDesignExpandedCells = 750_000;
    private const int GroundGapRadiusCells = 15;
    private const float GroundGapMaxRangeMeters = 12f;

    private readonly record struct PlanNode(int Cell, double G, double Z, double F);

    public DesignedRoadSearchResult FindDesignedRoad(
        double startX,
        double startY,
        double endX,
        double endY,
        double snapRadiusMeters,
        double maximumGradePercent,
        double maximumCutFillMeters,
        CancellationToken cancellationToken = default,
        RoadObjective objective = RoadObjective.Cost,
        IProgress<ElevationBuildProgress>? progress = null)
    {
        if (maximumGradePercent <= 0 || maximumCutFillMeters < 0)
            throw new ArgumentOutOfRangeException(
                nameof(maximumGradePercent), "경사 및 절토·성토 한도를 확인하세요.");

        if (!TrySnapCell(startX, startY, snapRadiusMeters,
                out int startIx, out int startIy))
            return new(null, "시점 주변에 고도가 없습니다.");

        if (!TrySnapCell(endX, endY, snapRadiusMeters,
                out int endIx, out int endIy))
            return new(null, "종점 주변에 고도가 없습니다.");

        int startKey = Key(startIx, startIy);
        int endKey = Key(endIx, endIy);
        double startZ = _cells[startKey];
        double endZ = _cells[endKey];

        if (startKey == endKey)
        {
            return new(
                [new RoadDesignVertex(
                    CenterX(startIx), CenterY(startIy), startZ, startZ)],
                null);
        }

        double gradeRatio = maximumGradePercent / 100.0;
        double minimumRequiredLengthMeters =
            Math.Abs(endZ - startZ) / gradeRatio;
        double straightDistanceMeters = Heuristic(
            startIx, startIy, endIx, endIy, CellSizeMeters);

        List<(int X, int Y)>? coarsePath = FindCoarseRoadCells(
            startIx, startIy, endIx, endIy,
            gradeRatio, maximumCutFillMeters,
            cancellationToken,
            out bool disconnected);

        if (coarsePath != null)
        {
            DesignedRoadSearchResult alongBypass = SearchDesignedPlan(
                startIx, startIy, startKey,
                endIx, endIy, endKey,
                startZ, endZ,
                gradeRatio,
                maximumGradePercent,
                maximumCutFillMeters,
                coarsePath,
                halfWidthMeters: 80,
                limitToBox: false,
                boxMinX: 0, boxMinY: 0, boxMaxX: Width - 1, boxMaxY: Height - 1,
                straightDistanceMeters,
                minimumRequiredLengthMeters,
                objective,
                progress,
                cancellationToken,
                out bool hitStateLimit);

            if (alongBypass.Succeeded || hitStateLimit)
                return alongBypass;
        }

        if (disconnected)
        {
            return new(null,
                "시점과 종점 사이의 고도 격자가 끊겨 있습니다. " +
                "지면 점이 비어 있는 구간이 넓어 임도를 이을 수 없습니다.");
        }

        double boxMargin = Math.Clamp(
            Math.Max(straightDistanceMeters, minimumRequiredLengthMeters),
            280,
            700);
        int marginCells = Math.Max(1, (int)Math.Ceiling(boxMargin / CellSizeMeters));
        int boxMinX = Math.Max(0, Math.Min(startIx, endIx) - marginCells);
        int boxMaxX = Math.Min(Width - 1, Math.Max(startIx, endIx) + marginCells);
        int boxMinY = Math.Max(0, Math.Min(startIy, endIy) - marginCells);
        int boxMaxY = Math.Min(Height - 1, Math.Max(startIy, endIy) + marginCells);

        DesignedRoadSearchResult boxed = SearchDesignedPlan(
            startIx, startIy, startKey,
            endIx, endIy, endKey,
            startZ, endZ,
            gradeRatio,
            maximumGradePercent,
            maximumCutFillMeters,
            coarsePath: null,
            halfWidthMeters: 0,
            limitToBox: true,
            boxMinX, boxMinY, boxMaxX, boxMaxY,
            straightDistanceMeters,
            minimumRequiredLengthMeters,
            objective,
            progress,
            cancellationToken,
            out _);

        if (boxed.Succeeded)
            return boxed;

        double heightGap = Math.Abs(endZ - startZ);
        return new(null,
            $"경사 {maximumGradePercent:0.#}%와 절토·성토 {maximumCutFillMeters:0.#}m 안에서 " +
            $"직선 {straightDistanceMeters:0.0}m, 고도차 {heightGap:0.0}m 구간을 우회해 이을 경로가 없습니다. " +
            "사이 지형이 급하면 직선 주변이 아니라 더 먼 우회로가 필요합니다. " +
            (boxed.Failure ?? ""));
    }

    private DesignedRoadSearchResult SearchDesignedPlan(
        int startIx,
        int startIy,
        int startKey,
        int endIx,
        int endIy,
        int endKey,
        double startZ,
        double endZ,
        double gradeRatio,
        double maximumGradePercent,
        double maximumCutFillMeters,
        List<(int X, int Y)>? coarsePath,
        double halfWidthMeters,
        bool limitToBox,
        int boxMinX,
        int boxMinY,
        int boxMaxX,
        int boxMaxY,
        double straightDistanceMeters,
        double minimumRequiredLengthMeters,
        RoadObjective objective,
        IProgress<ElevationBuildProgress>? progress,
        CancellationToken cancellationToken,
        out bool hitStateLimit)
    {
        hitStateLimit = false;

        bool[]? corridorMask = coarsePath == null
            ? null
            : BuildCoarseCorridorMask(coarsePath, halfWidthMeters);
        if (corridorMask != null)
        {
            corridorMask[startKey] = true;
            corridorMask[endKey] = true;
        }

        var bestCost = new Dictionary<int, double> { [startKey] = 0 };
        var bestCut = new Dictionary<int, double> { [startKey] = 0 };
        var parent = new Dictionary<int, int>();
        var roadZ = new Dictionary<int, double> { [startKey] = startZ };
        var open = new PriorityQueue<PlanNode, double>();
        var gapCache = new Dictionary<int, float>();

        double startRemaining = Math.Max(
            straightDistanceMeters,
            Math.Abs(endZ - startZ) / gradeRatio);
        open.Enqueue(new PlanNode(startKey, 0, startZ, startRemaining), startRemaining);

        int expanded = 0;

        while (open.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PlanNode node = open.Dequeue();
            if (!roadZ.TryGetValue(node.Cell, out double storedZ) ||
                Math.Abs(storedZ - node.Z) > 0.1)
                continue;
            if (bestCost.TryGetValue(node.Cell, out double storedCost) &&
                node.G > storedCost + 0.2)
                continue;

            if (node.Cell == endKey)
            {
                List<RoadDesignVertex>? traced = ReconstructPlan(
                    startKey, endKey, startZ, endZ, parent, roadZ);
                List<RoadDesignVertex>? fitted = traced == null
                    ? null
                    : FitGradeProfile(traced, gradeRatio, maximumCutFillMeters);

                if (fitted != null)
                    return new(fitted, null);

                continue;
            }

            if (++expanded > MaxDesignExpandedCells)
            {
                hitStateLimit = true;
                return new(null,
                    $"도로 탐색이 {MaxDesignExpandedCells:N0}칸에서 멈췄습니다. " +
                    $"직선거리 {straightDistanceMeters:0.0}m, " +
                    $"경사 기준 최소 길이 {minimumRequiredLengthMeters:0.0}m입니다. " +
                    "경로가 없다는 판정은 아닙니다.");
            }

            if ((expanded & 8191) == 0)
            {
                progress?.Report(new ElevationBuildProgress(
                    Math.Min(0.9, expanded / 200_000.0),
                    $"초안 경로 탐색 중... {expanded:N0}칸"));
            }

            int x = node.Cell % Width;
            int y = node.Cell / Width;
            var onPath = new HashSet<int> { node.Cell };
            int ancestor = node.Cell;
            for (int guard = 0;
                 guard < 4000 && parent.TryGetValue(ancestor, out int older);
                 guard++)
            {
                if (!onPath.Add(older))
                    break;
                ancestor = older;
            }

            foreach ((int dx, int dy) in Steps)
            {
                int nx = x + dx;
                int ny = y + dy;
                if ((uint)nx >= (uint)Width || (uint)ny >= (uint)Height)
                    continue;

                int nextKey = Key(nx, ny);
                if (limitToBox &&
                    (nx < boxMinX || nx > boxMaxX || ny < boxMinY || ny > boxMaxY))
                    continue;
                if (corridorMask != null && !corridorMask[nextKey])
                    continue;
                if (onPath.Contains(nextKey))
                    continue;

                if (!TryCachedGround(nx, ny, gapCache, out float ground))
                    continue;

                double distance = CellSizeMeters * Math.Sqrt(dx * dx + dy * dy);

                if (nextKey == endKey)
                {
                    if (Math.Abs(endZ - node.Z) <= gradeRatio * distance + 0.0001)
                    {
                        Consider(
                            endKey, node, endZ, distance, endZ,
                            gradeRatio, objective,
                            endIx, endIy, endZ,
                            bestCost, bestCut, parent, roadZ, open);
                    }

                    continue;
                }

                double remaining = Heuristic(nx, ny, endIx, endIy, CellSizeMeters);
                if (!TryChooseRoadZ(
                        node.Z,
                        ground,
                        distance,
                        gradeRatio,
                        maximumCutFillMeters,
                        out double nextZ))
                    continue;

                Consider(
                    nextKey, node, nextZ, distance, ground,
                    gradeRatio, objective,
                    endIx, endIy, endZ,
                    bestCost, bestCut, parent, roadZ, open);
            }
        }

        return new(null,
            $"경사 {maximumGradePercent:0.#}%와 절토·성토 " +
            $"{maximumCutFillMeters:0.#}m 안에서 이을 수 있는 초안 경로가 없습니다.");
    }

    private void Consider(
        int nextKey,
        PlanNode node,
        double nextRoadZ,
        double distance,
        double groundZ,
        double gradeRatio,
        RoadObjective objective,
        int endIx,
        int endIy,
        double endZ,
        Dictionary<int, double> bestCost,
        Dictionary<int, double> bestCut,
        Dictionary<int, int> parent,
        Dictionary<int, double> roadZ,
        PriorityQueue<PlanNode, double> open)
    {
        double allowed = gradeRatio * distance;
        double gradeFraction = allowed <= 1e-9
            ? 0
            : Math.Abs(nextRoadZ - node.Z) / allowed;
        double cutFillHeight = Math.Abs(nextRoadZ - groundZ);

        double stepWeight = objective switch
        {
            RoadObjective.Safety =>
                1.0 + 2.0 * cutFillHeight + 2.0 * gradeFraction,
            RoadObjective.Cost =>
                1.0 + 0.4 * cutFillHeight,
            RoadObjective.Distance =>
                1.0,
            RoadObjective.ConstructionTime =>
                1.0 + 0.15 * cutFillHeight + 0.3 * gradeFraction,
            _ => throw new ArgumentOutOfRangeException(nameof(objective))
        };

        double newCost = node.G + distance * stepWeight;
        int nx = nextKey % Width;
        int ny = nextKey / Width;
        double remainingXY = Heuristic(nx, ny, endIx, endIy, CellSizeMeters);
        double remainingHeight = Math.Abs(endZ - nextRoadZ) / gradeRatio;
        double priority = newCost + Math.Max(remainingXY, remainingHeight);
        bool seen = bestCut.TryGetValue(nextKey, out double previousCut);
        if (seen)
        {
            bool betterFit = cutFillHeight + 0.2 < previousCut;
            bool betterCost = newCost + 1.0 < bestCost[nextKey] &&
                              cutFillHeight <= previousCut + 0.05;
            if (!betterFit && !betterCost)
                return;
        }

        bestCost[nextKey] = newCost;
        bestCut[nextKey] = cutFillHeight;
        parent[nextKey] = node.Cell;
        roadZ[nextKey] = nextRoadZ;
        open.Enqueue(new PlanNode(nextKey, newCost, nextRoadZ, priority), priority);
    }

    private List<RoadDesignVertex>? ReconstructPlan(
        int startKey,
        int endKey,
        double startZ,
        double endZ,
        Dictionary<int, int> parent,
        Dictionary<int, double> roadZ)
    {
        var keys = new List<int>();
        int cursor = endKey;
        int guard = 0;
        int guardLimit = MaxDesignExpandedCells + 2;

        while (true)
        {
            keys.Add(cursor);
            if (cursor == startKey)
                break;

            if (!parent.TryGetValue(cursor, out int previous) ||
                ++guard > guardLimit)
                return null;

            cursor = previous;
        }

        keys.Reverse();
        var vertices = new List<RoadDesignVertex>(keys.Count);

        for (int i = 0; i < keys.Count; i++)
        {
            int key = keys[i];
            int ix = key % Width;
            int iy = key / Width;
            double z = i == 0
                ? startZ
                : i == keys.Count - 1
                    ? endZ
                    : roadZ[key];

            float ground = i == 0
                ? (float)startZ
                : i == keys.Count - 1
                    ? (float)endZ
                    : _cells[key];

            if (float.IsNaN(ground) &&
                !TrySampleGround(ix, iy, out ground))
                ground = (float)z;

            vertices.Add(new RoadDesignVertex(
                CenterX(ix),
                CenterY(iy),
                ground,
                z));
        }

        return vertices;
    }

    private static List<RoadDesignVertex>? FitGradeProfile(
        IReadOnlyList<RoadDesignVertex> draft,
        double gradeRatio,
        double maximumCutFillMeters)
    {
        int count = draft.Count;
        if (count == 0)
            return null;
        if (count == 1)
            return [draft[0]];

        var step = new double[count];
        var lowReach = new double[count];
        var highReach = new double[count];
        double startZ = draft[0].RoadZ;
        double endZ = draft[^1].RoadZ;
        lowReach[0] = highReach[0] = startZ;

        for (int i = 1; i < count; i++)
        {
            double dx = draft[i].X - draft[i - 1].X;
            double dy = draft[i].Y - draft[i - 1].Y;
            step[i] = Math.Sqrt(dx * dx + dy * dy);
            if (step[i] < 1e-4)
                step[i] = 1e-4;

            double allowed = gradeRatio * step[i];
            double low = Math.Max(
                lowReach[i - 1] - allowed,
                draft[i].GroundZ - maximumCutFillMeters);
            double high = Math.Min(
                highReach[i - 1] + allowed,
                draft[i].GroundZ + maximumCutFillMeters);

            if (i == count - 1)
            {
                low = Math.Max(low, endZ);
                high = Math.Min(high, endZ);
            }

            if (low > high + 0.001)
                return null;
            if (low > high)
                low = high;

            lowReach[i] = low;
            highReach[i] = high;
        }

        var road = new double[count];
        road[count - 1] = endZ;

        for (int i = count - 2; i >= 0; i--)
        {
            double allowed = gradeRatio * step[i + 1];
            double low = Math.Max(lowReach[i], road[i + 1] - allowed);
            double high = Math.Min(highReach[i], road[i + 1] + allowed);
            low = Math.Max(low, draft[i].GroundZ - maximumCutFillMeters);
            high = Math.Min(high, draft[i].GroundZ + maximumCutFillMeters);

            if (i == 0)
            {
                low = Math.Max(low, startZ);
                high = Math.Min(high, startZ);
            }

            if (low > high + 0.001)
                return null;
            if (low > high)
                low = high;

            double chosen = draft[i].GroundZ;
            if (chosen < low)
                chosen = low;
            else if (chosen > high)
                chosen = high;
            road[i] = chosen;
        }

        if (Math.Abs(road[0] - startZ) > 0.05)
            return null;

        var fitted = new List<RoadDesignVertex>(count);
        for (int i = 0; i < count; i++)
        {
            fitted.Add(new RoadDesignVertex(
                draft[i].X,
                draft[i].Y,
                draft[i].GroundZ,
                road[i]));
        }

        return fitted;
    }

    private bool TryCachedGround(
        int ix,
        int iy,
        Dictionary<int, float> gapCache,
        out float z)
    {
        int index = iy * Width + ix;
        float direct = _cells[index];
        if (!float.IsNaN(direct))
        {
            z = direct;
            return true;
        }

        if (gapCache.TryGetValue(index, out z))
            return !float.IsNaN(z);

        if (!TrySampleGround(ix, iy, out z))
        {
            gapCache[index] = float.NaN;
            return false;
        }

        gapCache[index] = z;
        return true;
    }

    private bool TrySampleGroundAt(double x, double y, out float z)
    {
        z = 0;
        if (!double.IsFinite(x) || !double.IsFinite(y))
            return false;

        int ix = (int)Math.Floor((x - _minX) / CellSizeMeters);
        int iy = (int)Math.Floor((y - _minY) / CellSizeMeters);
        if (ix == Width)
            ix--;
        if (iy == Height)
            iy--;

        return TrySampleGround(ix, iy, out z);
    }

    private bool TrySampleGround(int ix, int iy, out float z)
    {
        z = 0;
        if ((uint)ix >= (uint)Width || (uint)iy >= (uint)Height)
            return false;

        float direct = _cells[iy * Width + ix];
        if (!float.IsNaN(direct))
        {
            z = direct;
            return true;
        }

        double weightSum = 0;
        double zSum = 0;
        int count = 0;
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int dy = -GroundGapRadiusCells; dy <= GroundGapRadiusCells; dy++)
        {
            int y = iy + dy;
            if ((uint)y >= (uint)Height)
                continue;

            int row = y * Width;
            for (int dx = -GroundGapRadiusCells; dx <= GroundGapRadiusCells; dx++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int x = ix + dx;
                if ((uint)x >= (uint)Width)
                    continue;

                float sample = _cells[row + x];
                if (float.IsNaN(sample))
                    continue;

                double distance = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.5);
                double weight = 1.0 / distance;
                weightSum += weight;
                zSum += weight * sample;
                count++;
                if (sample < min)
                    min = sample;
                if (sample > max)
                    max = sample;
            }
        }

        if (count < 2 || max - min > GroundGapMaxRangeMeters || weightSum <= 0)
            return false;

        z = (float)(zSum / weightSum);
        return true;
    }

    private static bool TryChooseRoadZ(
        double currentZ,
        double groundZ,
        double distance,
        double gradeRatio,
        double maximumCutFillMeters,
        out double nextZ)
    {
        double allowed = Math.Max(0, gradeRatio * distance);
        double low = Math.Max(currentZ - allowed, groundZ - maximumCutFillMeters);
        double high = Math.Min(currentZ + allowed, groundZ + maximumCutFillMeters);

        // 지면 높이는 float라서 하한이 상한보다 1mm 미만으로 뒤집힐 수 있다.
        if (low > high)
        {
            if (low - high > 0.001)
            {
                nextZ = 0;
                return false;
            }

            nextZ = high;
            return true;
        }

        // 경사와 절토·성토가 허용하는 범위에서 지면에 붙인다.
        // 직선이 너무 급하면 사면을 비스듬히 오르는 우회가 이 높이를 유지한다.
        nextZ = groundZ;
        if (nextZ < low)
            nextZ = low;
        else if (nextZ > high)
            nextZ = high;
        return true;
    }
}
