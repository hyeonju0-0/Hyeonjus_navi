namespace find_forestLoad.Las;

public sealed partial class ElevationGrid
{
    private readonly object _coarseGate = new();
    private int _cachedCoarseStart = int.MinValue;
    private int _cachedCoarseEnd = int.MinValue;
    private int _cachedGradeBits = int.MinValue;
    private int _cachedCutFillBits = int.MinValue;
    private List<(int X, int Y)>? _cachedCoarsePath;
    private bool _cachedDisconnected;
    private bool _hasCoarseCache;

    private List<(int X, int Y)>? FindCoarseRoadCells(
        int startIx, int startIy,
        int endIx, int endIy,
        double gradeRatio,
        double maximumCutFillMeters,
        CancellationToken cancellationToken,
        out bool disconnected)
    {
        lock (_coarseGate)
        {
            return FindCoarseRoadCellsLocked(
                startIx, startIy, endIx, endIy,
                gradeRatio, maximumCutFillMeters,
                cancellationToken,
                out disconnected);
        }
    }

    private List<(int X, int Y)>? FindCoarseRoadCellsLocked(
        int startIx, int startIy,
        int endIx, int endIy,
        double gradeRatio,
        double maximumCutFillMeters,
        CancellationToken cancellationToken,
        out bool disconnected)
    {
        const int blockSize = 4;

        int coarseWidth = (Width + blockSize - 1) / blockSize;
        int coarseHeight = (Height + blockSize - 1) / blockSize;
        int count = coarseWidth * coarseHeight;
        int start = (startIy / blockSize) * coarseWidth
                  + startIx / blockSize;
        int end = (endIy / blockSize) * coarseWidth
                + endIx / blockSize;
        int gradeBits = (int)Math.Round(gradeRatio * 100_000);
        int cutFillBits = (int)Math.Round(maximumCutFillMeters * 1000);

        if (_hasCoarseCache &&
            _cachedCoarseStart == start &&
            _cachedCoarseEnd == end &&
            _cachedGradeBits == gradeBits &&
            _cachedCutFillBits == cutFillBits)
        {
            disconnected = _cachedDisconnected;
            return _cachedCoarsePath;
        }

        var available = new bool[count];
        var ground = new float[count];
        Array.Fill(ground, float.NaN);
        var sum = new double[count];
        var samples = new int[count];

        for (int iy = 0; iy < Height; iy++)
        {
            int coarseRow = (iy / blockSize) * coarseWidth;
            int fineRow = iy * Width;

            for (int ix = 0; ix < Width; ix++)
            {
                float z = _cells[fineRow + ix];
                if (float.IsNaN(z))
                    continue;

                int block = coarseRow + ix / blockSize;
                sum[block] += z;
                samples[block]++;
                available[block] = true;
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (samples[i] > 0)
                ground[i] = (float)(sum[i] / samples[i]);
        }

        var bridged = (bool[])available.Clone();
        var bridgedGround = (float[])ground.Clone();
        for (int i = 0; i < count; i++)
        {
            if (available[i])
                continue;

            int cellX = i % coarseWidth;
            int cellY = i / coarseWidth;
            double neighborSum = 0;
            int neighborCount = 0;
            float neighborMin = float.MaxValue;
            float neighborMax = float.MinValue;

            for (int dy = -1; dy <= 1; dy++)
            {
                int ny = cellY + dy;
                if ((uint)ny >= (uint)coarseHeight)
                    continue;

                int row = ny * coarseWidth;
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = cellX + dx;
                    if ((uint)nx >= (uint)coarseWidth || (dx == 0 && dy == 0))
                        continue;

                    int neighbor = row + nx;
                    if (!available[neighbor])
                        continue;

                    float sample = ground[neighbor];
                    neighborSum += sample;
                    neighborCount++;
                    if (sample < neighborMin)
                        neighborMin = sample;
                    if (sample > neighborMax)
                        neighborMax = sample;
                }
            }

            if (neighborCount >= 2 && neighborMax - neighborMin <= 8f)
            {
                bridged[i] = true;
                bridgedGround[i] = (float)(neighborSum / neighborCount);
            }
        }

        available = bridged;
        ground = bridgedGround;

        double straight = Math.Sqrt(
            Math.Pow((endIx - startIx) * CellSizeMeters, 2) +
            Math.Pow((endIy - startIy) * CellSizeMeters, 2));
        double boxMargin = Math.Clamp(Math.Max(straight, 200) * 1.15, 350, 800);
        int margin = Math.Max(1, (int)Math.Ceiling(
            boxMargin / (blockSize * CellSizeMeters)));
        int startBlockX = startIx / blockSize;
        int startBlockY = startIy / blockSize;
        int endBlockX = endIx / blockSize;
        int endBlockY = endIy / blockSize;
        int minX = Math.Max(0, Math.Min(startBlockX, endBlockX) - margin);
        int maxX = Math.Min(coarseWidth - 1, Math.Max(startBlockX, endBlockX) + margin);
        int minY = Math.Max(0, Math.Min(startBlockY, endBlockY) - margin);
        int maxY = Math.Min(coarseHeight - 1, Math.Max(startBlockY, endBlockY) + margin);

        List<(int X, int Y)>? slopePath = SearchCoarse(
            respectSlope: true, out _);
        if (slopePath != null)
        {
            Remember(slopePath, isDisconnected: false);
            disconnected = false;
            return slopePath;
        }

        List<(int X, int Y)>? openPath = SearchCoarse(
            respectSlope: false, out _);
        disconnected = openPath == null;
        Remember(null, disconnected);
        return null;

        void Remember(List<(int X, int Y)>? path, bool isDisconnected)
        {
            _cachedCoarseStart = start;
            _cachedCoarseEnd = end;
            _cachedGradeBits = gradeBits;
            _cachedCutFillBits = cutFillBits;
            _cachedCoarsePath = path;
            _cachedDisconnected = isDisconnected;
            _hasCoarseCache = true;
        }

        List<(int X, int Y)>? SearchCoarse(bool respectSlope, out bool found)
        {
            found = false;
            var best = new double[count];
            Array.Fill(best, double.PositiveInfinity);
            var previous = new int[count];
            Array.Fill(previous, -1);
            var closed = new bool[count];
            var open = new PriorityQueue<int, double>();

            if ((uint)start >= (uint)count || !available[start])
                return null;

            best[start] = 0;
            open.Enqueue(start, 0);

            (int Dx, int Dy)[] steps =
            [
                (-1, 0), (1, 0), (0, -1), (0, 1),
                (-1, -1), (-1, 1), (1, -1), (1, 1)
            ];

            while (open.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int current = open.Dequeue();
                if (closed[current])
                    continue;

                closed[current] = true;
                if (current == end)
                {
                    var path = new List<(int X, int Y)>();
                    for (int cell = end; cell != -1; cell = previous[cell])
                        path.Add((cell % coarseWidth, cell / coarseWidth));

                    path.Reverse();
                    found = true;
                    return path;
                }

                int x = current % coarseWidth;
                int y = current / coarseWidth;
                float currentGround = ground[current];

                foreach ((int dx, int dy) in steps)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < minX || nx > maxX || ny < minY || ny > maxY)
                        continue;

                    int next = ny * coarseWidth + nx;
                    if (!available[next] || closed[next])
                        continue;

                    double stepCells = Math.Sqrt(dx * dx + dy * dy);
                    if (respectSlope)
                    {
                        float nextGround = ground[next];
                        if (float.IsNaN(currentGround) || float.IsNaN(nextGround))
                            continue;

                        double stepMeters = stepCells * blockSize * CellSizeMeters;
                        double climbLimit =
                            gradeRatio * stepMeters + 2 * maximumCutFillMeters;
                        if (Math.Abs(nextGround - currentGround) > climbLimit + 0.05)
                            continue;
                    }

                    double extraClimb = 0;
                    if (!float.IsNaN(currentGround) && !float.IsNaN(ground[next]))
                    {
                        double rise = Math.Abs(ground[next] - currentGround);
                        double stepMeters = stepCells * blockSize * CellSizeMeters;
                        extraClimb = Math.Max(0, rise - gradeRatio * stepMeters);
                    }

                    double newCost = best[current] + stepCells + extraClimb;
                    if (newCost >= best[next])
                        continue;

                    best[next] = newCost;
                    previous[next] = current;

                    double remainingX = nx - endBlockX;
                    double remainingY = ny - endBlockY;
                    double remaining = Math.Sqrt(
                        remainingX * remainingX + remainingY * remainingY);
                    open.Enqueue(next, newCost + remaining);
                }
            }

            return null;
        }
    }


    private bool[] BuildCoarseCorridorMask(
    IReadOnlyList<(int X, int Y)> coarsePath,
    double halfWidthMeters)
    {
        const int blockSize = 4;

        var mask = new bool[_cells.Length];
        int margin = (int)Math.Ceiling(halfWidthMeters / CellSizeMeters);

        foreach ((int coarseX, int coarseY) in coarsePath)
        {
            int minX = Math.Max(0, coarseX * blockSize - margin);
            int maxX = Math.Min(
                Width - 1, (coarseX + 1) * blockSize - 1 + margin);

            int minY = Math.Max(0, coarseY * blockSize - margin);
            int maxY = Math.Min(
                Height - 1, (coarseY + 1) * blockSize - 1 + margin);

            for (int iy = minY; iy <= maxY; iy++)
            {
                int row = iy * Width;

                for (int ix = minX; ix <= maxX; ix++)
                    mask[row + ix] = true;
            }
        }

        return mask;
    }
}