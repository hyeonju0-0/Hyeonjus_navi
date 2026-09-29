namespace find_forestLoad.Las
{
    public readonly record struct RoadVertex(double X, double Y, double Z);

    /// <summary>
    /// 한 지점의 원래 지면 높이와 설계할 도로 높이.
    /// </summary>
    public readonly record struct RoadDesignVertex(
        double X,
        double Y,
        double GroundZ,
        double RoadZ)
    {
        // 도로가 지면보다 낮으면 깎아야 하는 높이
        public double CutHeight => Math.Max(0, GroundZ - RoadZ);

        // 도로가 지면보다 높으면 메워야 하는 높이
        public double FillHeight => Math.Max(0, RoadZ - GroundZ);
    }

    public readonly record struct RoadSegment(
        RoadVertex From,
        RoadVertex To,
        double HorizontalMeters,
        double SlopePercent);

    public sealed class ForestRoadPath
    {
        internal ForestRoadPath(
            IReadOnlyList<RoadVertex> vertices,
            IReadOnlyList<RoadSegment> segments,
            double maxStepSlopePercent,
            int expandedCells)
        {
            Vertices = vertices;
            Segments = segments;
            MaxStepSlopePercent = maxStepSlopePercent;
            ExpandedCells = expandedCells;
            LengthMeters = 0;
            MaxSlopePercent = 0;
            foreach (RoadSegment segment in segments)
            {
                LengthMeters += segment.HorizontalMeters;
                double abs = Math.Abs(segment.SlopePercent);
                if (abs > MaxSlopePercent)
                    MaxSlopePercent = abs;
            }
        }

        public IReadOnlyList<RoadVertex> Vertices { get; }

        public IReadOnlyList<RoadSegment> Segments { get; }

        public double LengthMeters { get; }

        public double MaxSlopePercent { get; }

        public double MaxStepSlopePercent { get; }

        public int ExpandedCells { get; }
    }

    public readonly record struct ForestRoadResult(ForestRoadPath? Path, string? Failure)
    {
        public bool Succeeded => Path != null;
    }
}
