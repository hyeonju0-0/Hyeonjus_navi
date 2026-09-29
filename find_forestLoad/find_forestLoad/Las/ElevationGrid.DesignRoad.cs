namespace find_forestLoad.Las;

public sealed record DesignedRoadSearchResult(
    IReadOnlyList<RoadDesignVertex>? Vertices,
    string? Failure)
{
    public bool Succeeded => Vertices is { Count: > 0 };
}

internal readonly record struct DesignSearchState(int CellKey, int HeightLevel);

public sealed partial class ElevationGrid
{
    private const double RoadHeightInterval = 0.05; // 도로 높이 탐색 간격: 2.5cm
    private const int MaxDesignSearchStates = 1_500_000;
    private const int EndHeightLevel = int.MinValue;

    public DesignedRoadSearchResult FindDesignedRoad(
        double startX,
        double startY,
        double endX,
        double endY,
        double snapRadiusMeters,
        double maximumGradePercent,
        double maximumCutFillMeters,
        CancellationToken cancellationToken = default,
        RoadObjective objective = RoadObjective.Cost)
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

        List<(int X, int Y)>? coarsePath =
            FindCoarseRoadCells(
                startIx, startIy, endIx, endIy,
                cancellationToken);

        bool[]? corridorMask = coarsePath == null
            ? null
            : BuildCoarseCorridorMask(coarsePath, 40.0);

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

        // 시점과 종점의 고도 차이를 허용 경사로 극복하는 데 필요한 최소 수평 길이
        double minimumRequiredLengthMeters =
            Math.Abs(endZ - startZ) / gradeRatio;

        double dxToEnd = (endIx - startIx) * CellSizeMeters;
        double dyToEnd = (endIy - startIy) * CellSizeMeters;

        double straightDistanceMeters =
            Math.Sqrt(dxToEnd * dxToEnd + dyToEnd * dyToEnd);

        var start = new DesignSearchState(startKey, 0);
        var goal = new DesignSearchState(endKey, EndHeightLevel);

        var open = new PriorityQueue<DesignSearchState, double>();
        var bestCost = new Dictionary<DesignSearchState, double>
        {
            [start] = 0
        };
        var parent = new Dictionary<DesignSearchState, DesignSearchState>();
        var closed = new HashSet<DesignSearchState>();

        double startRemainingXY =
            Heuristic(startIx, startIy, endIx, endIy, CellSizeMeters);

        double startRemainingHeight =
            Math.Abs(endZ - startZ) / gradeRatio;

        open.Enqueue(start,
            Math.Max(startRemainingXY, startRemainingHeight));

        int expanded = 0;

        while (open.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DesignSearchState current = open.Dequeue();

            if (!closed.Add(current))
                continue;

            if (current == goal)
            {
                var states = new List<DesignSearchState> { current };

                while (current != start)
                {
                    current = parent[current];
                    states.Add(current);
                }

                states.Reverse();

                var vertices = new List<RoadDesignVertex>(states.Count);

                foreach (DesignSearchState state in states)
                {
                    int ix = state.CellKey % Width;
                    int iy = state.CellKey / Width;

                    double roadZ = state == goal
                        ? endZ
                        : startZ + state.HeightLevel * RoadHeightInterval;

                    vertices.Add(new RoadDesignVertex(
                        CenterX(ix),
                        CenterY(iy),
                        GroundZ: _cells[state.CellKey],
                        RoadZ: roadZ));
                }

                return new(vertices, null);
            }

            if (++expanded > MaxDesignSearchStates)
            {
                return new(null,
                    $"도로 높이 탐색이 {MaxDesignSearchStates:N0}개 상태 제한에 도달했습니다. " +
                    $"두 점의 직선거리 {straightDistanceMeters:0.0}m, " +
                    $"고도 차이와 경사 기준으로 필요한 최소 길이 " +
                    $"{minimumRequiredLengthMeters:0.0}m입니다. " +
                    "경로가 없다는 판정은 아닙니다.");
            }

            int x = current.CellKey % Width;
            int y = current.CellKey / Width;
            double currentZ =
                startZ + current.HeightLevel * RoadHeightInterval;
            double currentCost = bestCost[current];

            foreach ((int dx, int dy) in Steps)
            {
                int nx = x + dx;
                int ny = y + dy;

                if ((uint)nx >= (uint)Width ||
                    (uint)ny >= (uint)Height)
                    continue;

                int nextKey = Key(nx, ny);

                if (corridorMask != null && !corridorMask[nextKey])
                    continue;

                double groundZ = _cells[nextKey];

                if (double.IsNaN(groundZ))
                    continue;

                // 현재 경로에서 이미 지나온 XY 격자 칸은 다시 방문하지 않는다.
                bool alreadyVisitedOnThisPath = false;
                DesignSearchState ancestor = current;

                while (true)
                {
                    if (ancestor.CellKey == nextKey)
                    {
                        alreadyVisitedOnThisPath = true;
                        break;
                    }

                    if (ancestor == start)
                        break;

                    ancestor = parent[ancestor];
                }

                if (alreadyVisitedOnThisPath)
                    continue;

                double distance =
                    CellSizeMeters * Math.Sqrt(dx * dx + dy * dy);
                double allowedHeightChange = gradeRatio * distance;

                if (nextKey == endKey)
                {
                    if (Math.Abs(endZ - currentZ) <=
                        allowedHeightChange + 0.000001)
                    {
                        AddNeighbor(goal, endZ);
                    }

                    continue;
                }

                // 경사 조건과 절토·성토 조건을 모두 만족하는
                // 다음 칸의 도로 높이 후보만 살핀다.
                double lowest = Math.Max(
                    currentZ - allowedHeightChange,
                    groundZ - maximumCutFillMeters);

                double highest = Math.Min(
                    currentZ + allowedHeightChange,
                    groundZ + maximumCutFillMeters);

                int firstLevel = (int)Math.Ceiling(
                    (lowest - startZ) / RoadHeightInterval - 0.000001);

                int lastLevel = (int)Math.Floor(
                    (highest - startZ) / RoadHeightInterval + 0.000001);

                for (int level = firstLevel; level <= lastLevel; level++)
                {
                    double roadZ =
                        startZ + level * RoadHeightInterval;

                    AddNeighbor(
                        new DesignSearchState(nextKey, level),
                        roadZ);
                }

                void AddNeighbor(
                    DesignSearchState neighbor,
                    double nextRoadZ)
                {
                    if (closed.Contains(neighbor))
                        return;

                    double cutFillHeight =
                        Math.Abs(nextRoadZ - groundZ);

                    // 길이 + 절토·성토 높이에 대한 탐색용 가중치.
                    // 실제 공사비나 토공량은 아직 아니다.
                    double gradeFraction = Math.Abs(nextRoadZ - currentZ) / allowedHeightChange;

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

                    double newCost = currentCost + distance * stepWeight;

                    if (bestCost.TryGetValue(
                            neighbor, out double previousCost) &&
                        newCost >= previousCost)
                        return;

                    bestCost[neighbor] = newCost;
                    parent[neighbor] = current;

                    double remainingXY =
                        Heuristic(nx, ny, endIx, endIy, CellSizeMeters);

                    double remainingHeight =
                        Math.Abs(endZ - nextRoadZ) / gradeRatio;

                    double priority =
                        newCost + Math.Max(remainingXY, remainingHeight);

                    open.Enqueue(neighbor, priority);
                }
            }
        }

        return new(null,
            $"경사 {maximumGradePercent:0.#}%와 절토·성토 " +
            $"{maximumCutFillMeters:0.#}m 조건을 만족하는 초안 경로가 없습니다.");
    }
}