namespace find_forestLoad.Las
{
    public sealed class LasHeader
    {
        internal LasHeader()
        {
        }

        public int VersionMajor { get; internal set; }

        public int VersionMinor { get; internal set; }

        public int PointDataFormat { get; internal set; }

        public int PointDataRecordLength { get; internal set; }

        public long PointCount { get; internal set; }

        public double ScaleX { get; internal set; }

        public double ScaleY { get; internal set; }

        public double ScaleZ { get; internal set; }

        public double OffsetX { get; internal set; }

        public double OffsetY { get; internal set; }

        public double OffsetZ { get; internal set; }

        public double MinX { get; internal set; }

        public double MaxX { get; internal set; }

        public double MinY { get; internal set; }

        public double MaxY { get; internal set; }

        public double MinZ { get; internal set; }

        public double MaxZ { get; internal set; }

        public int HeaderSize { get; internal set; }

        public long PointDataOffset { get; internal set; }

        public string GeneratingSoftware { get; internal set; } = "";

        public string? CrsWkt { get; internal set; }
    }
}
