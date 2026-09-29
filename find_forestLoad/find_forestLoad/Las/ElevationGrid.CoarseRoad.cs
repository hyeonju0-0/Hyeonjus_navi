namespace find_forestLoad.Las;

public sealed partial class ElevationGrid
{
    private List<(int X, int Y)>? FindCoarseRoadCells(
        int startIx, int startIy,
        int endIx, int endIy,
        CancellationToken cancellationToken)
    {
        const int blockSize = 4;

        int coarseWidth = (Width + blockSize - 1) / blockSize;
        int coarseHeight = (Height + blockSize - 1) / blockSize;
        int count = coarseWidth * coarseHeight;

        var available = new bool[count];

        for (int iy = 0; iy < Height; iy++)
        {
            int coarseRow = (iy / blockSize) * coarseWidth;
            int fineRow = iy * Width;

            for (int ix = 0; ix < Width; ix++)
            {
                if (!float.IsNaN(_cells[fineRow + ix]))
                    available[coarseRow + ix / blockSize] = true;
            }
        }

        int start = (startIy / blockSize) * coarseWidth
                  + startIx / blockSize;
        int end = (endIy / blockSize) * coarseWidth
                + endIx / blockSize;

        var best = new double[count];
        Array.Fill(best, double.PositiveInfinity);

        var previous = new int[count];
        Array.Fill(previous, -1);

        var closed = new bool[count];
        var open = new PriorityQueue<int, double>();

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
                return path;
            }

            int x = current % coarseWidth;
            int y = current / coarseWidth;

            foreach ((int dx, int dy) in steps)
            {
                int nx = x + dx;
                int ny = y + dy;

                if ((uint)nx >= (uint)coarseWidth ||
                    (uint)ny >= (uint)coarseHeight)
                    continue;

                int next = ny * coarseWidth + nx;

                if (!available[next] || closed[next])
                    continue;

                double distance = Math.Sqrt(dx * dx + dy * dy);
                double newCost = best[current] + distance;

                if (newCost >= best[next])
                    continue;

                best[next] = newCost;
                previous[next] = current;

                double remainingX = nx - endIx / blockSize;
                double remainingY = ny - endIy / blockSize;
                double remaining =
                    Math.Sqrt(remainingX * remainingX +
                              remainingY * remainingY);

                open.Enqueue(next, newCost + remaining);
            }
        }

        return null;
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