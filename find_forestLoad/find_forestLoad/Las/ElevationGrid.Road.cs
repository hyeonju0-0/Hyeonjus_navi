namespace find_forestLoad.Las
{
    public sealed partial class ElevationGrid
    {
        private const int MaxExpandedCells = 1_500_000;

        private static readonly (int Dx, int Dy)[] Steps =
        [
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (1, 1), (1, -1), (-1, 1), (-1, -1)
        ];

        public ForestRoadResult FindRoad(
            double startX,
            double startY,
            double endX,
            double endY,
            double snapRadiusMeters,
            double allowedSlopePercent,
            IProgress<ElevationBuildProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!double.IsFinite(allowedSlopePercent) || allowedSlopePercent <= 0 || allowedSlopePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(allowedSlopePercent), "허용 경사는 0보다 크고 100% 이하여야 합니다.");

            if (!TrySnapCell(startX, startY, snapRadiusMeters, out int startIx, out int startIy))
                return new ForestRoadResult(null, "시점 반경 안에 고도가 없습니다.");

            if (!TrySnapCell(endX, endY, snapRadiusMeters, out int endIx, out int endIy))
                return new ForestRoadResult(null, "종점 반경 안에 고도가 없습니다.");

            int startKey = Key(startIx, startIy);
            int endKey = Key(endIx, endIy);
            if (startKey == endKey)
            {
                var only = new RoadVertex(CenterX(startIx), CenterY(startIy), _cells[startKey]);
                return new ForestRoadResult(new ForestRoadPath([only], [], 0, 0), null);
            }

            double allowedRatio = allowedSlopePercent / 100.0;
            double cell = CellSizeMeters;
            var gScore = new Dictionary<int, double> { [startKey] = 0 };
            var parent = new Dictionary<int, int>();
            var closed = new HashSet<int>();
            var open = new PriorityQueue<int, double>();
            open.Enqueue(startKey, Heuristic(startIx, startIy, endIx, endIy, cell));

            int expanded = 0;
            bool found = false;
            while (open.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int current = open.Dequeue();
                if (!closed.Add(current))
                    continue;

                if (current == endKey)
                {
                    found = true;
                    break;
                }

                expanded++;
                if (expanded > MaxExpandedCells)
                {
                    return new ForestRoadResult(null, "허용 경사 안에서 탐색 범위가 너무 넓습니다. 경사 한계를 올리거나 시점과 종점을 더 가깝게 잡으세요.");
                }

                if ((expanded & 16383) == 0)
                {
                    progress?.Report(new ElevationBuildProgress(0, $"임도 경로를 찾는 중... {expanded:N0}칸"));
                }

                int ix = current % Width;
                int iy = current / Width;
                float z = _cells[current];
                double g = gScore[current];

                foreach ((int dx, int dy) in Steps)
                {
                    int nx = ix + dx;
                    int ny = iy + dy;
                    if ((uint)nx >= (uint)Width || (uint)ny >= (uint)Height)
                        continue;

                    int neighbor = Key(nx, ny);
                    if (closed.Contains(neighbor))
                        continue;

                    float nz = _cells[neighbor];
                    if (float.IsNaN(nz))
                        continue;

                    double step = cell * Math.Sqrt(dx * dx + dy * dy);
                    double grade = Math.Abs(nz - z) / step;
                    if (grade > allowedRatio)
                        continue;

                    double tentative = g + step;
                    if (gScore.TryGetValue(neighbor, out double known) && tentative >= known)
                        continue;

                    parent[neighbor] = current;
                    gScore[neighbor] = tentative;
                    double priority = tentative + Heuristic(nx, ny, endIx, endIy, cell);
                    open.Enqueue(neighbor, priority);
                }
            }

            if (!found)
                return new ForestRoadResult(null, "허용 경사 안에서는 시점과 종점을 잇는 경로가 없습니다.");

            List<int> cells = Reconstruct(startKey, endKey, parent);
            double maxStep = MaxStepSlope(cells, cell);
            List<int> turns = Simplify(cells, Width);
            var vertices = new List<RoadVertex>(turns.Count);
            foreach (int key in turns)
            {
                int ix = key % Width;
                int iy = key / Width;
                vertices.Add(new RoadVertex(CenterX(ix), CenterY(iy), _cells[key]));
            }

            var segments = new List<RoadSegment>(vertices.Count - 1);
            for (int i = 1; i < vertices.Count; i++)
            {
                RoadVertex from = vertices[i - 1];
                RoadVertex to = vertices[i];
                double horizontal = Math.Sqrt((to.X - from.X) * (to.X - from.X) + (to.Y - from.Y) * (to.Y - from.Y));
                double slope = horizontal < 1e-6 ? 0 : (to.Z - from.Z) / horizontal * 100.0;
                segments.Add(new RoadSegment(from, to, horizontal, slope));
            }

            progress?.Report(new ElevationBuildProgress(1, "임도 경로를 찾았습니다."));
            return new ForestRoadResult(new ForestRoadPath(vertices, segments, maxStep, expanded), null);
        }

        private List<int> Reconstruct(int startKey, int endKey, Dictionary<int, int> parent)
        {
            var cells = new List<int>();
            int cursor = endKey;
            cells.Add(cursor);
            while (cursor != startKey)
            {
                cursor = parent[cursor];
                cells.Add(cursor);
            }

            cells.Reverse();
            return cells;
        }

        private double MaxStepSlope(List<int> cells, double cell)
        {
            double max = 0;
            for (int i = 1; i < cells.Count; i++)
            {
                int previous = cells[i - 1];
                int current = cells[i];
                int dx = current % Width - previous % Width;
                int dy = current / Width - previous / Width;
                double step = cell * Math.Sqrt(dx * dx + dy * dy);
                double grade = Math.Abs(_cells[current] - _cells[previous]) / step * 100.0;
                if (grade > max)
                    max = grade;
            }

            return max;
        }

        private static List<int> Simplify(List<int> cells, int width)
        {
            if (cells.Count <= 2)
                return cells;

            var kept = new List<int> { cells[0] };
            for (int i = 1; i < cells.Count - 1; i++)
            {
                int previous = cells[i - 1];
                int current = cells[i];
                int next = cells[i + 1];
                int dx1 = current % width - previous % width;
                int dy1 = current / width - previous / width;
                int dx2 = next % width - current % width;
                int dy2 = next / width - current / width;
                if (dx1 != dx2 || dy1 != dy2)
                    kept.Add(current);
            }

            kept.Add(cells[^1]);
            return kept;
        }

        private bool TrySnapCell(double x, double y, double radius, out int ix, out int iy)
        {
            ix = 0;
            iy = 0;
            if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(radius) || radius <= 0)
                return false;

            double cell = CellSizeMeters;
            int minIx = (int)Math.Floor((x - radius - _minX) / cell) - 1;
            int maxIx = (int)Math.Floor((x + radius - _minX) / cell) + 1;
            int minIy = (int)Math.Floor((y - radius - _minY) / cell) - 1;
            int maxIy = (int)Math.Floor((y + radius - _minY) / cell) + 1;
            if (maxIx < 0 || maxIy < 0 || minIx >= Width || minIy >= Height)
                return false;

            if (minIx < 0) minIx = 0;
            if (minIy < 0) minIy = 0;
            if (maxIx >= Width) maxIx = Width - 1;
            if (maxIy >= Height) maxIy = Height - 1;

            double best = double.MaxValue;
            bool found = false;
            for (int cy = minIy; cy <= maxIy; cy++)
            {
                int row = cy * Width;
                double y0 = _minY + cy * cell;
                for (int cx = minIx; cx <= maxIx; cx++)
                {
                    if (float.IsNaN(_cells[row + cx]))
                        continue;

                    double x0 = _minX + cx * cell;
                    if (DistanceToRect(x, y, x0, y0, x0 + cell, y0 + cell) > radius)
                        continue;

                    double dx = x0 + cell * 0.5 - x;
                    double dy = y0 + cell * 0.5 - y;
                    double distance = dx * dx + dy * dy;
                    if (distance < best)
                    {
                        best = distance;
                        ix = cx;
                        iy = cy;
                        found = true;
                    }
                }
            }

            return found;
        }

        private int Key(int ix, int iy) => iy * Width + ix;

        private double CenterX(int ix) => _minX + (ix + 0.5) * CellSizeMeters;

        private double CenterY(int iy) => _minY + (iy + 0.5) * CellSizeMeters;

        private static double Heuristic(int ix, int iy, int goalIx, int goalIy, double cell)
        {
            double dx = (ix - goalIx) * cell;
            double dy = (iy - goalIy) * cell;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
