namespace find_forestLoad.Las
{
    public readonly record struct ElevationBuildProgress(double Fraction, string Message);

    public readonly record struct ElevationSample(double X, double Y, double Z, double RadiusMeters, int CellCount);

    /// <summary>
    /// LAS 점을 한 번 읽어 칸마다 최저 Z만 남긴 격자. 입력 좌표 주변 고도는 이 격자에서 가져온다.
    /// </summary>
    public sealed partial class ElevationGrid
    {
        private const int MaxCells = 60_000_000;
        private const byte GroundClass = 2;
        private const byte LowNoiseClass = 7;
        private const byte HighNoiseClass = 18;

        private readonly float[] _cells;
        private readonly double _minX;
        private readonly double _minY;

        private ElevationGrid(LasHeader header, string sourcePath, double cellSizeMeters, int width, int height, float[] cells)
        {
            Header = header;
            SourcePath = sourcePath;
            CellSizeMeters = cellSizeMeters;
            Width = width;
            Height = height;
            _minX = header.MinX;
            _minY = header.MinY;
            _cells = cells;
        }

        public LasHeader Header { get; }

        public string SourcePath { get; }

        public double CellSizeMeters { get; }

        public int Width { get; }

        public int Height { get; }

        public bool UsedGroundClass { get; private set; }

        public long PointsUsed { get; private set; }

        public long GroundPointCount { get; private set; }

        public long OutOfBoundsCount { get; private set; }

        public string ElevationRule =>
            UsedGroundClass
                ? "지면점(분류 2)의 최저 Z"
                : "지면 분류가 거의 없어, 노이즈(분류 7·18)를 뺀 점의 최저 Z";

        public static Task<ElevationGrid> BuildAsync(
            string path,
            double cellSizeMeters = 1,
            IProgress<ElevationBuildProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Build(path, cellSizeMeters, progress, cancellationToken), cancellationToken);
        }

        public ElevationSample? GetElevation(double x, double y, double radiusMeters)
        {
            if (!double.IsFinite(x) || !double.IsFinite(y))
                return null;

            if (!double.IsFinite(radiusMeters) || radiusMeters <= 0 || radiusMeters > 500)
                throw new ArgumentOutOfRangeException(nameof(radiusMeters), "검색 반경은 0보다 크고 500m 이하여야 합니다.");

            double cell = CellSizeMeters;
            int minIx = (int)Math.Floor((x - radiusMeters - _minX) / cell) - 1;
            int maxIx = (int)Math.Floor((x + radiusMeters - _minX) / cell) + 1;
            int minIy = (int)Math.Floor((y - radiusMeters - _minY) / cell) - 1;
            int maxIy = (int)Math.Floor((y + radiusMeters - _minY) / cell) + 1;

            if (maxIx < 0 || maxIy < 0 || minIx >= Width || minIy >= Height)
                return null;

            if (minIx < 0) minIx = 0;
            if (minIy < 0) minIy = 0;
            if (maxIx >= Width) maxIx = Width - 1;
            if (maxIy >= Height) maxIy = Height - 1;

            long window = (long)(maxIx - minIx + 1) * (maxIy - minIy + 1);
            if (window > 2_000_000)
                throw new ArgumentOutOfRangeException(nameof(radiusMeters), "검색 반경이 너무 큽니다.");

            double weightSum = 0;
            double zSum = 0;
            int count = 0;

            for (int iy = minIy; iy <= maxIy; iy++)
            {
                int row = iy * Width;
                double y0 = _minY + iy * cell;
                double y1 = y0 + cell;
                for (int ix = minIx; ix <= maxIx; ix++)
                {
                    float z = _cells[row + ix];
                    if (float.IsNaN(z))
                        continue;

                    double x0 = _minX + ix * cell;
                    double x1 = x0 + cell;
                    if (DistanceToRect(x, y, x0, y0, x1, y1) > radiusMeters)
                        continue;

                    double dx = x0 + cell * 0.5 - x;
                    double dy = y0 + cell * 0.5 - y;
                    double influence = Math.Max(Math.Sqrt(dx * dx + dy * dy), cell * 0.25);
                    double weight = 1.0 / (influence * influence);
                    weightSum += weight;
                    zSum += weight * z;
                    count++;
                }
            }

            if (count == 0 || weightSum == 0)
                return null;

            return new ElevationSample(x, y, zSum / weightSum, radiusMeters, count);
        }

        private static ElevationGrid Build(
            string path,
            double cellSizeMeters,
            IProgress<ElevationBuildProgress>? progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ElevationBuildProgress(0, "LAS 헤더를 읽는 중..."));
            LasHeader header = LasReader.ReadHeader(path);

            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ElevationBuildProgress(0, "점 분류를 확인하는 중..."));
            bool groundOnly;
            using (FileStream stream = LasReader.Open(path))
            {
                groundOnly = HasEnoughGround(stream, header, cancellationToken);
            }

            var (width, height, cell) = FitGrid(header.MinX, header.MaxX, header.MinY, header.MaxY, cellSizeMeters);
            progress?.Report(new ElevationBuildProgress(0, "고도 격자를 준비하는 중..."));

            float[] cells;
            try
            {
                cells = new float[width * (long)height];
            }
            catch (OutOfMemoryException)
            {
                throw new InvalidDataException("고도 격자를 만들 메모리가 부족합니다.");
            }

            Array.Fill(cells, float.NaN);
            var grid = new ElevationGrid(header, Path.GetFullPath(path), cell, width, height, cells);

            using (FileStream stream = LasReader.Open(path))
            {
                var sink = new GridFill(grid, groundOnly);
                string phase = groundOnly ? "지면 고도를 읽는 중" : "전체 점 고도를 읽는 중";
                LasReader.ReadPoints(stream, header, 0, header.PointCount, ref sink, phase, progress, cancellationToken);

                if (groundOnly && sink.PointsUsed == 0)
                {
                    if (sink.GroundPoints > 0)
                        throw new InvalidDataException("지면점이 헤더의 X/Y 범위 밖에 있습니다.");

                    progress?.Report(new ElevationBuildProgress(0, "지면 분류가 없어 전체 점을 다시 읽는 중..."));
                    grid.ClearCells();
                    groundOnly = false;
                    sink = new GridFill(grid, false);
                    LasReader.ReadPoints(stream, header, 0, header.PointCount, ref sink, "전체 점 고도를 읽는 중", progress, cancellationToken);
                }

                if (sink.PointsUsed == 0)
                {
                    if (sink.OutOfBounds > 0)
                        throw new InvalidDataException("점이 헤더의 X/Y 범위 밖에 있습니다.");
                    throw new InvalidDataException("고도로 사용할 점이 없습니다.");
                }

                grid.UsedGroundClass = groundOnly;
                grid.PointsUsed = sink.PointsUsed;
                grid.GroundPointCount = sink.GroundPoints;
                grid.OutOfBoundsCount = sink.OutOfBounds;
            }

            progress?.Report(new ElevationBuildProgress(1, "고도 격자를 만들었습니다."));
            return grid;
        }

        private static bool HasEnoughGround(FileStream stream, LasHeader header, CancellationToken cancellationToken)
        {
            long window = Math.Min(20_000, header.PointCount);
            int origins = header.PointCount > window ? 3 : 1;
            long total = 0;
            long ground = 0;

            for (int i = 0; i < origins; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                long start = origins == 1 ? 0 : (header.PointCount - window) * i / (origins - 1);
                var probe = new ClassProbe();
                LasReader.ReadPoints(stream, header, start, window, ref probe, null, null, cancellationToken);
                total += probe.Total;
                ground += probe.Ground;
            }

            return total > 0 && ground * 100 >= total;
        }

        private static (int Width, int Height, double CellSize) FitGrid(
            double minX,
            double maxX,
            double minY,
            double maxY,
            double requestedCell)
        {
            if (!double.IsFinite(requestedCell) || requestedCell <= 0)
                throw new ArgumentOutOfRangeException(nameof(requestedCell), "격자 크기는 0보다 커야 합니다.");

            double spanX = maxX - minX;
            double spanY = maxY - minY;
            double cell = requestedCell;

            for (int attempt = 0; attempt < 32; attempt++)
            {
                double width = spanX <= 0 ? 1 : Math.Ceiling(spanX / cell - 1e-9);
                double height = spanY <= 0 ? 1 : Math.Ceiling(spanY / cell - 1e-9);
                if (width < 1) width = 1;
                if (height < 1) height = 1;
                if (width <= int.MaxValue && height <= int.MaxValue && width * height <= MaxCells)
                    return ((int)width, (int)height, cell);

                cell *= 2;
            }

            throw new InvalidDataException("LAS 범위가 너무 넓어 고도 격자를 만들 수 없습니다.");
        }

        private bool TryAccumulate(double x, double y, double z)
        {
            int ix = (int)Math.Floor((x - _minX) / CellSizeMeters);
            int iy = (int)Math.Floor((y - _minY) / CellSizeMeters);
            if (ix == Width) ix--;
            if (iy == Height) iy--;
            if ((uint)ix >= (uint)Width || (uint)iy >= (uint)Height)
                return false;

            ref float slot = ref _cells[iy * Width + ix];
            float height = (float)z;
            if (float.IsNaN(slot) || height < slot)
                slot = height;
            return true;
        }

        private void ClearCells() => Array.Fill(_cells, float.NaN);

        private static double DistanceToRect(double x, double y, double x0, double y0, double x1, double y1)
        {
            double dx = x < x0 ? x0 - x : x > x1 ? x - x1 : 0;
            double dy = y < y0 ? y0 - y : y > y1 ? y - y1 : 0;
            if (dx == 0 && dy == 0)
                return 0;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private struct ClassProbe : ILasPointConsumer
        {
            public long Total;
            public long Ground;

            public void OnPoint(double x, double y, double z, byte classification, bool withheld)
            {
                if (withheld || classification == LowNoiseClass || classification == HighNoiseClass)
                    return;

                Total++;
                if (classification == GroundClass)
                    Ground++;
            }
        }

        private struct GridFill : ILasPointConsumer
        {
            private readonly ElevationGrid _grid;
            private readonly bool _groundOnly;

            public GridFill(ElevationGrid grid, bool groundOnly)
            {
                _grid = grid;
                _groundOnly = groundOnly;
            }

            public long PointsUsed;
            public long GroundPoints;
            public long OutOfBounds;

            public void OnPoint(double x, double y, double z, byte classification, bool withheld)
            {
                if (withheld || classification == LowNoiseClass || classification == HighNoiseClass)
                    return;

                if (classification == GroundClass)
                    GroundPoints++;

                if (_groundOnly && classification != GroundClass)
                    return;

                if (_grid.TryAccumulate(x, y, z))
                    PointsUsed++;
                else
                    OutOfBounds++;
            }
        }
    }
}
