namespace find_forestLoad.Las
{
    /// <summary>
    /// 화면 위쪽이 북쪽인 고도 표본. 빈 칸은 NaN이다.
    /// </summary>
    public readonly record struct TerrainOverview(
        int Width,
        int Height,
        float[] Samples,
        float MinZ,
        float MaxZ,
        double FilledMinX,
        double FilledMinY,
        double FilledMaxX,
        double FilledMaxY,
        int FilledCells);

    public sealed partial class ElevationGrid
    {
        public double GridMinX => _minX;

        public double GridMinY => _minY;

        public double GridMaxX => _minX + Width * CellSizeMeters;

        public double GridMaxY => _minY + Height * CellSizeMeters;

        public bool TryGetCellZ(double x, double y, out double z)
        {
            z = 0;
            if (!double.IsFinite(x) || !double.IsFinite(y))
                return false;

            int ix = (int)Math.Floor((x - _minX) / CellSizeMeters);
            int iy = (int)Math.Floor((y - _minY) / CellSizeMeters);
            if ((uint)ix >= (uint)Width || (uint)iy >= (uint)Height)
                return false;

            float stored = _cells[iy * Width + ix];
            if (float.IsNaN(stored))
                return false;

            z = stored;
            return true;
        }

        /// <summary>
        /// 전체 격자를 긴 변이 maxLongSide 이하가 되도록 줄인다. 한 픽셀에는 그 칸들의 최저 Z를 남긴다.
        /// </summary>
        public TerrainOverview BuildOverview(int maxLongSide, CancellationToken cancellationToken = default)
        {
            if (maxLongSide < 1)
                throw new ArgumentOutOfRangeException(nameof(maxLongSide));

            cancellationToken.ThrowIfCancellationRequested();
            int overviewWidth;
            int overviewHeight;
            if (Width >= Height)
            {
                overviewWidth = Math.Min(Width, maxLongSide);
                overviewHeight = Math.Max(1, (int)Math.Round(overviewWidth * (double)Height / Width));
            }
            else
            {
                overviewHeight = Math.Min(Height, maxLongSide);
                overviewWidth = Math.Max(1, (int)Math.Round(overviewHeight * (double)Width / Height));
            }

            int chunks = (long)Width * Height < 2_000_000
                ? 1
                : Math.Min(8, Math.Min(Environment.ProcessorCount, Height));

            var parts = new OverviewPart[chunks];
            Parallel.For(
                0,
                chunks,
                new ParallelOptions { CancellationToken = cancellationToken },
                chunk =>
                {
                    var samples = new float[overviewWidth * overviewHeight];
                    Array.Fill(samples, float.NaN);
                    var part = new OverviewPart
                    {
                        Samples = samples,
                        MinIx = int.MaxValue,
                        MinIy = int.MaxValue,
                        MaxIx = -1,
                        MaxIy = -1,
                        MinZ = float.PositiveInfinity,
                        MaxZ = float.NegativeInfinity
                    };
                    int y0 = Height * chunk / chunks;
                    int y1 = Height * (chunk + 1) / chunks;
                    FillOverviewRows(y0, y1, overviewWidth, overviewHeight, ref part, cancellationToken);
                    parts[chunk] = part;
                });

            float[] merged = parts[0].Samples;
            int minIx = parts[0].MinIx;
            int minIy = parts[0].MinIy;
            int maxIx = parts[0].MaxIx;
            int maxIy = parts[0].MaxIy;
            float minZ = parts[0].MinZ;
            float maxZ = parts[0].MaxZ;
            int filled = parts[0].FilledCells;

            for (int chunk = 1; chunk < parts.Length; chunk++)
            {
                OverviewPart part = parts[chunk];
                filled += part.FilledCells;
                if (part.MinIx < minIx) minIx = part.MinIx;
                if (part.MinIy < minIy) minIy = part.MinIy;
                if (part.MaxIx > maxIx) maxIx = part.MaxIx;
                if (part.MaxIy > maxIy) maxIy = part.MaxIy;
                if (part.MinZ < minZ) minZ = part.MinZ;
                if (part.MaxZ > maxZ) maxZ = part.MaxZ;

                float[] source = part.Samples;
                for (int i = 0; i < merged.Length; i++)
                {
                    float z = source[i];
                    if (float.IsNaN(z))
                        continue;
                    if (float.IsNaN(merged[i]) || z < merged[i])
                        merged[i] = z;
                }
            }

            if (maxIx < minIx || float.IsPositiveInfinity(minZ))
                throw new InvalidDataException("고도로 사용할 점이 없습니다.");

            double cell = CellSizeMeters;
            return new TerrainOverview(
                overviewWidth,
                overviewHeight,
                merged,
                minZ,
                maxZ,
                _minX + minIx * cell,
                _minY + minIy * cell,
                _minX + (maxIx + 1) * cell,
                _minY + (maxIy + 1) * cell,
                filled);
        }

        /// <summary>
        /// [minX, maxX) × [minY, maxY)를 화면 위쪽이 북쪽이 되도록 destination에 담는다.
        /// 한 픽셀보다 칸이 많으면 그 픽셀의 최저 Z를 쓰고, 확대되면 픽셀 중심의 칸을 쓴다.
        /// </summary>
        public void SampleWindow(
            double minX,
            double minY,
            double maxX,
            double maxY,
            int width,
            int height,
            float[] destination,
            CancellationToken cancellationToken = default)
        {
            if (width < 1 || height < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (destination.Length != width * height)
                throw new ArgumentException("표본 배열 길이가 가로×세로와 다릅니다.", nameof(destination));
            if (!double.IsFinite(minX) || !double.IsFinite(minY) || !double.IsFinite(maxX) || !double.IsFinite(maxY)
                || maxX <= minX || maxY <= minY)
                throw new ArgumentOutOfRangeException(nameof(minX));

            Array.Fill(destination, float.NaN);
            cancellationToken.ThrowIfCancellationRequested();

            CellRange(minX, maxX, _minX, CellSizeMeters, Width, out int ix0, out int ix1);
            CellRange(minY, maxY, _minY, CellSizeMeters, Height, out int iy0, out int iy1);
            if (ix1 < ix0 || iy1 < iy0)
                return;

            long cellsWide = ix1 - ix0 + 1L;
            long cellsHigh = iy1 - iy0 + 1L;
            if (cellsWide <= width && cellsHigh <= height)
            {
                SampleCenters(minX, minY, maxX, maxY, width, height, destination);
                return;
            }

            int stride = 1;
            while (stride < 64 && (cellsWide / stride) * (cellsHigh / stride) > 4_000_000)
                stride *= 2;

            double spanX = maxX - minX;
            double spanY = maxY - minY;
            double cell = CellSizeMeters;
            for (int iy = iy0; iy <= iy1; iy += stride)
            {
                if ((iy & 63) == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                double centerY = _minY + (iy + 0.5) * cell;
                if (centerY < minY || centerY >= maxY)
                    continue;

                int py = (int)((maxY - centerY) / spanY * height);
                if ((uint)py >= (uint)height)
                    continue;

                int sourceRow = iy * Width;
                int destRow = py * width;
                for (int ix = ix0; ix <= ix1; ix += stride)
                {
                    float z = _cells[sourceRow + ix];
                    if (float.IsNaN(z))
                        continue;

                    double centerX = _minX + (ix + 0.5) * cell;
                    if (centerX < minX || centerX >= maxX)
                        continue;

                    int px = (int)((centerX - minX) / spanX * width);
                    if ((uint)px >= (uint)width)
                        continue;

                    ref float slot = ref destination[destRow + px];
                    if (float.IsNaN(slot) || z < slot)
                        slot = z;
                }
            }
        }

        private void SampleCenters(
            double minX,
            double minY,
            double maxX,
            double maxY,
            int width,
            int height,
            float[] destination)
        {
            double spanX = maxX - minX;
            double spanY = maxY - minY;
            double cell = CellSizeMeters;
            for (int py = 0; py < height; py++)
            {
                double worldY = maxY - (py + 0.5) / height * spanY;
                int iy = (int)Math.Floor((worldY - _minY) / cell);
                if ((uint)iy >= (uint)Height)
                    continue;

                int sourceRow = iy * Width;
                int destRow = py * width;
                for (int px = 0; px < width; px++)
                {
                    double worldX = minX + (px + 0.5) / width * spanX;
                    int ix = (int)Math.Floor((worldX - _minX) / cell);
                    if ((uint)ix >= (uint)Width)
                        continue;

                    destination[destRow + px] = _cells[sourceRow + ix];
                }
            }
        }

        private void FillOverviewRows(
            int y0,
            int y1,
            int overviewWidth,
            int overviewHeight,
            ref OverviewPart part,
            CancellationToken cancellationToken)
        {
            float[] samples = part.Samples;
            for (int iy = y0; iy < y1; iy++)
            {
                if ((iy & 63) == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                int py = overviewHeight - 1 - (int)((long)iy * overviewHeight / Height);
                int sourceRow = iy * Width;
                int destRow = py * overviewWidth;
                for (int ix = 0; ix < Width; ix++)
                {
                    float z = _cells[sourceRow + ix];
                    if (float.IsNaN(z))
                        continue;

                    part.FilledCells++;
                    if (ix < part.MinIx) part.MinIx = ix;
                    if (iy < part.MinIy) part.MinIy = iy;
                    if (ix > part.MaxIx) part.MaxIx = ix;
                    if (iy > part.MaxIy) part.MaxIy = iy;
                    if (z < part.MinZ) part.MinZ = z;
                    if (z > part.MaxZ) part.MaxZ = z;

                    int px = (int)((long)ix * overviewWidth / Width);
                    ref float slot = ref samples[destRow + px];
                    if (float.IsNaN(slot) || z < slot)
                        slot = z;
                }
            }
        }

        private static void CellRange(
            double min,
            double max,
            double origin,
            double cell,
            int count,
            out int i0,
            out int i1)
        {
            i0 = (int)Math.Floor((min - origin) / cell);
            i1 = (int)Math.Floor((max - origin) / cell - 1e-9);
            if (i0 < 0) i0 = 0;
            if (i1 >= count) i1 = count - 1;
        }

        private struct OverviewPart
        {
            public float[] Samples;
            public int MinIx;
            public int MinIy;
            public int MaxIx;
            public int MaxIy;
            public float MinZ;
            public float MaxZ;
            public int FilledCells;
        }
    }
}
