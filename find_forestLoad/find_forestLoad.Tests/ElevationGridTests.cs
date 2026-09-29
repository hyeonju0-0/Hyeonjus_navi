using System.Buffers.Binary;
using System.Text;
using find_forestLoad.Las;
using Xunit;

namespace find_forestLoad.Tests
{
    public class ElevationGridTests
    {
        [Fact]
        public async Task Format0_UsesGroundMinimum_AndNearbyCell()
        {
            using var tmp = new TempLas();
            const double scaleX = 0.01;
            const double scaleY = 0.001;
            const double scaleZ = 0.1;
            const double offX = 1000;
            const double offY = 2000;
            const double offZ = 10;

            byte[][] points =
            [
                PointFormat0(Enc(1010.2, scaleX, offX), Enc(2020.2, scaleY, offY), Enc(12, scaleZ, offZ), 2, false, 24),
                PointFormat0(Enc(1010.8, scaleX, offX), Enc(2020.8, scaleY, offY), Enc(15, scaleZ, offZ), 2, false, 24),
                PointFormat0(Enc(1010.4, scaleX, offX), Enc(2020.4, scaleY, offY), Enc(3, scaleZ, offZ), 5, false, 24),
                PointFormat0(Enc(1010.1, scaleX, offX), Enc(2020.1, scaleY, offY), Enc(1, scaleZ, offZ), 2, true, 24),
                PointFormat0(Enc(1030.2, scaleX, offX), Enc(2020.2, scaleY, offY), Enc(40, scaleZ, offZ), 2, false, 24)
            ];

            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 24,
                legacyCount: 5,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX, scaleY, scaleZ,
                offX, offY, offZ,
                minX: 1000, maxX: 1100, minY: 2000, maxY: 2100, minZ: 0, maxZ: 100);
            Save(tmp.Path, header, null, points);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);

            Assert.Equal(0, grid.Header.PointDataFormat);
            Assert.Equal(5, grid.Header.PointCount);
            Assert.Equal(0.01, grid.Header.ScaleX, 6);
            Assert.Equal(0.001, grid.Header.ScaleY, 6);
            Assert.Equal(0.1, grid.Header.ScaleZ, 6);
            Assert.Equal(1000, grid.Header.OffsetX, 3);
            Assert.Equal(10, grid.Header.OffsetZ, 3);
            Assert.Equal(1100, grid.Header.MaxX, 3);
            Assert.Equal(1000, grid.Header.MinX, 3);
            Assert.Equal("unit-test", grid.Header.GeneratingSoftware);
            Assert.True(grid.UsedGroundClass);

            ElevationSample? local = grid.GetElevation(1010.5, 2020.5, 0.5);
            Assert.True(local.HasValue);
            Assert.Equal(12, local.Value.Z, 3);

            ElevationSample? other = grid.GetElevation(1030.2, 2020.2, 0.5);
            Assert.True(other.HasValue);
            Assert.Equal(40, other.Value.Z, 3);

            ElevationSample? neighbor = grid.GetElevation(1012.5, 2020.5, 2);
            Assert.True(neighbor.HasValue);
            Assert.Equal(12, neighbor.Value.Z, 3);
            Assert.Null(grid.GetElevation(1012.5, 2020.5, 0.5));
            Assert.False(grid.GetElevation(double.NaN, 2020.5, 1).HasValue);
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetElevation(1010.5, 2020.5, 0));
        }

        [Fact]
        public async Task PointOnMaxBound_IsKept()
        {
            using var tmp = new TempLas();
            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: 1,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 1, minY: 0, maxY: 1, minZ: 0, maxZ: 100);
            byte[] point = PointFormat0(Enc(1, 0.001, 0), Enc(1, 0.001, 0), Enc(42, 0.001, 0), 2, false, 20);
            Save(tmp.Path, header, null, [point]);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            ElevationSample? sample = grid.GetElevation(0.2, 0.2, 0.5);

            Assert.Equal(0, grid.OutOfBoundsCount);
            Assert.True(sample.HasValue);
            Assert.Equal(42, sample.Value.Z, 3);
        }

        [Fact]
        public async Task Format6_ReadsClassificationByte_AndExtendedCount()
        {
            using var tmp = new TempLas();
            const double scaleX = 0.01;
            const double scaleY = 0.001;
            const double scaleZ = 0.1;
            const double offX = 1000;
            const double offY = 2000;
            const double offZ = 10;

            byte[] ground = PointFormat6(Enc(1010.5, scaleX, offX), Enc(2020.5, scaleY, offY), Enc(15, scaleZ, offZ), classification: 2, flags: 0);
            byte[] looksLikeGroundInOldLayout = PointFormat6(Enc(1010.6, scaleX, offX), Enc(2020.6, scaleY, offY), Enc(3, scaleZ, offZ), classification: 5, flags: 2);
            byte[] withheldGround = PointFormat6(Enc(1010.7, scaleX, offX), Enc(2020.7, scaleY, offY), Enc(1, scaleZ, offZ), classification: 2, flags: 0x04);

            byte[] header = CreateHeader(
                minor: 4,
                headerSize: 375,
                format: 6,
                recordLength: 30,
                legacyCount: 0,
                extendedCount: 3,
                pointOffset: 375,
                vlrCount: 0,
                scaleX, scaleY, scaleZ,
                offX, offY, offZ,
                minX: 1000, maxX: 1100, minY: 2000, maxY: 2100, minZ: 0, maxZ: 100);
            Save(tmp.Path, header, null, [ground, looksLikeGroundInOldLayout, withheldGround]);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            ElevationSample? sample = grid.GetElevation(1010.5, 2020.5, 0.5);

            Assert.Equal(4, grid.Header.VersionMinor);
            Assert.Equal(6, grid.Header.PointDataFormat);
            Assert.Equal(3, grid.Header.PointCount);
            Assert.True(grid.UsedGroundClass);
            Assert.True(sample.HasValue);
            Assert.Equal(15, sample.Value.Z, 3);
        }

        [Fact]
        public async Task WithoutGroundClass_UsesLowestNonNoisePoint()
        {
            using var tmp = new TempLas();
            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: 2,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 20, minY: 0, maxY: 20, minZ: -200, maxZ: 100);
            byte[] unclassified = PointFormat0(Enc(5, 0.001, 0), Enc(5, 0.001, 0), Enc(40, 0.001, 0), 1, false, 20);
            byte[] noise = PointFormat0(Enc(5.2, 0.001, 0), Enc(5.2, 0.001, 0), Enc(-100, 0.001, 0), 7, false, 20);
            Save(tmp.Path, header, null, [unclassified, noise]);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            ElevationSample? sample = grid.GetElevation(5.1, 5.1, 1);

            Assert.False(grid.UsedGroundClass);
            Assert.True(sample.HasValue);
            Assert.Equal(40, sample.Value.Z, 3);
        }

        [Fact]
        public async Task WktVlr_DoesNotShiftPointData()
        {
            using var tmp = new TempLas();
            byte[] wkt = Encoding.UTF8.GetBytes("PROJCS[\"Korea 2000\"]\0");
            byte[] vlr = CreateVlr("LASF_Projection", 2112, wkt);
            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: 1,
                extendedCount: 0,
                pointOffset: (uint)(227 + vlr.Length),
                vlrCount: 1,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 20, minY: 0, maxY: 20, minZ: 0, maxZ: 20);
            byte[] point = PointFormat0(Enc(4, 0.001, 0), Enc(6, 0.001, 0), Enc(8, 0.001, 0), 2, false, 20);
            Save(tmp.Path, header, vlr, [point]);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            ElevationSample? sample = grid.GetElevation(4, 6, 1);

            Assert.Contains("Korea 2000", grid.Header.CrsWkt);
            Assert.True(sample.HasValue);
            Assert.Equal(8, sample.Value.Z, 3);
        }

        [Fact]
        public async Task LaszipVlr_IsRejected()
        {
            using var tmp = new TempLas();
            byte[] vlr = CreateVlr("laszip encoded", 22204, new byte[8]);
            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: 1,
                extendedCount: 0,
                pointOffset: (uint)(227 + vlr.Length),
                vlrCount: 1,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 10, minY: 0, maxY: 10, minZ: 0, maxZ: 10);
            byte[] point = PointFormat0(1000, 1000, 1000, 2, false, 20);
            Save(tmp.Path, header, vlr, [point]);

            InvalidDataException ex = await Assert.ThrowsAsync<InvalidDataException>(() => ElevationGrid.BuildAsync(tmp.Path));
            Assert.Contains("LAZ", ex.Message);
        }

        [Fact]
        public async Task LazExtension_IsRejected()
        {
            using var tmp = new TempLas(".laz");
            await File.WriteAllBytesAsync(tmp.Path, [1, 2, 3, 4]);

            InvalidDataException ex = await Assert.ThrowsAsync<InvalidDataException>(() => ElevationGrid.BuildAsync(tmp.Path));
            Assert.Contains("LAZ", ex.Message);
        }

        [Fact]
        public async Task BadSignature_IsRejected()
        {
            using var tmp = new TempLas();
            await File.WriteAllBytesAsync(tmp.Path, new byte[227]);

            InvalidDataException ex = await Assert.ThrowsAsync<InvalidDataException>(() => ElevationGrid.BuildAsync(tmp.Path));
            Assert.Contains("아닙니다", ex.Message);
        }

        [Fact]
        public async Task Road_FollowsGentleRamp_WithinSlopeLimit()
        {
            using var tmp = new TempLas();
            var points = new List<byte[]>();
            for (int i = 0; i <= 9; i++)
            {
                points.Add(PointFormat0(
                    Enc(i + 0.5, 0.001, 0),
                    Enc(2.5, 0.001, 0),
                    Enc(i * 0.05, 0.001, 0),
                    2,
                    false,
                    20));
            }

            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: (uint)points.Count,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 12, minY: 0, maxY: 6, minZ: 0, maxZ: 1);
            Save(tmp.Path, header, null, points);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            ForestRoadResult road = grid.FindRoad(0.5, 2.5, 9.5, 2.5, 1, 10);

            Assert.True(road.Succeeded);
            Assert.NotNull(road.Path);
            Assert.Equal(2, road.Path.Vertices.Count);
            Assert.InRange(road.Path.MaxStepSlopePercent, 4, 6);
            Assert.InRange(Math.Abs(road.Path.Segments[0].SlopePercent), 4, 6);
        }

        [Fact]
        public async Task Road_DetoursAroundCliff_WhenSlopeWouldExceedLimit()
        {
            using var tmp = new TempLas();
            var points = new List<byte[]>();
            for (int x = 0; x <= 7; x++)
            {
                for (int y = 1; y <= 3; y++)
                {
                    double z = x == 4 && y == 2 ? 20 : 0;
                    points.Add(PointFormat0(
                        Enc(x + 0.5, 0.001, 0),
                        Enc(y + 0.5, 0.001, 0),
                        Enc(z, 0.001, 0),
                        2,
                        false,
                        20));
                }
            }

            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: (uint)points.Count,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 10, minY: 0, maxY: 6, minZ: 0, maxZ: 20);
            Save(tmp.Path, header, null, points);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            ForestRoadResult road = grid.FindRoad(0.5, 2.5, 7.5, 2.5, 1, 10);

            Assert.True(road.Succeeded);
            Assert.NotNull(road.Path);
            Assert.True(road.Path.LengthMeters > 7.5);
            Assert.True(road.Path.MaxStepSlopePercent < 1);
            Assert.All(road.Path.Vertices, vertex => Assert.True(vertex.Z < 1));
        }

        [Fact]
        public async Task Road_Fails_WhenOnlyConnectionIsTooSteep()
        {
            using var tmp = new TempLas();
            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: 2,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 3, minY: 0, maxY: 3, minZ: 0, maxZ: 10);
            byte[] low = PointFormat0(Enc(0.5, 0.001, 0), Enc(0.5, 0.001, 0), Enc(0, 0.001, 0), 2, false, 20);
            byte[] high = PointFormat0(Enc(1.5, 0.001, 0), Enc(0.5, 0.001, 0), Enc(10, 0.001, 0), 2, false, 20);
            Save(tmp.Path, header, null, [low, high]);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            ForestRoadResult road = grid.FindRoad(0.5, 0.5, 1.5, 0.5, 1, 10);

            Assert.False(road.Succeeded);
            Assert.Null(road.Path);
            Assert.Contains("않습니다", road.Failure);
        }

        [Fact]
        public async Task DesignedRoad_FollowsGentleSlope_AndCrossesShortGap()
        {
            using var tmp = new TempLas();
            var points = new List<byte[]>();

            for (int x = 0; x <= 12; x++)
            {
                if (x is >= 5 and <= 7)
                    continue;

                points.Add(PointFormat0(
                    Enc(x + 0.5, 0.001, 0),
                    Enc(1.5, 0.001, 0),
                    Enc(x * 0.04, 0.001, 0),
                    2,
                    false,
                    20));
            }

            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: (uint)points.Count,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 14, minY: 0, maxY: 4, minZ: 0, maxZ: 2);
            Save(tmp.Path, header, null, points);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            DesignedRoadSearchResult road = grid.FindDesignedRoad(
                0.5, 1.5, 12.5, 1.5, 1, 12, 3);

            Assert.True(road.Succeeded, road.Failure);
            Assert.NotNull(road.Vertices);
            Assert.True(road.Vertices.Count > 8);
            Assert.InRange(road.Vertices[0].RoadZ, -0.01, 0.01);
            Assert.InRange(road.Vertices[^1].RoadZ, 0.47, 0.49);

            bool crossedGap = road.Vertices.Any(point => point.X > 5 && point.X < 8);
            Assert.True(crossedGap);
        }

        [Fact]
        public async Task DesignedRoad_DoesNotThrow_WhenFloatGroundPinchesHeightBand()
        {
            using var tmp = new TempLas();
            var points = new List<byte[]>();
            const double baseZ = 151.78884887695312;

            for (int x = 0; x <= 30; x++)
            {
                points.Add(PointFormat0(
                    Enc(x + 0.5, 0.001, 0),
                    Enc(2.5, 0.001, 0),
                    Enc(baseZ + x * 0.03, 0.001, 0),
                    2,
                    false,
                    20));
            }

            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: (uint)points.Count,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 32, minY: 0, maxY: 6, minZ: 140, maxZ: 160);
            Save(tmp.Path, header, null, points);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            DesignedRoadSearchResult road = grid.FindDesignedRoad(
                0.5, 2.5, 30.5, 2.5, 1, 8, 3);

            Assert.True(road.Succeeded, road.Failure);
            Assert.NotNull(road.Vertices);
            Assert.InRange(road.Vertices[^1].RoadZ, baseZ + 0.8, baseZ + 1.0);
        }

        [Fact]
        public async Task DesignedRoad_GoesAroundSteepPit()
        {
            using var tmp = new TempLas();
            var points = new List<byte[]>();

            for (int x = 0; x <= 24; x++)
            {
                for (int y = 0; y <= 16; y++)
                {
                    bool pit = x is >= 11 and <= 13 && y is >= 6 and <= 10;
                    points.Add(PointFormat0(
                        Enc(x + 0.5, 0.001, 0),
                        Enc(y + 0.5, 0.001, 0),
                        Enc(pit ? 0 : 20, 0.001, 0),
                        2,
                        false,
                        20));
                }
            }

            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: (uint)points.Count,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 26, minY: 0, maxY: 18, minZ: 0, maxZ: 30);
            Save(tmp.Path, header, null, points);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            DesignedRoadSearchResult road = grid.FindDesignedRoad(
                0.5, 8.5, 24.5, 8.5, 1, 9, 3);

            Assert.True(road.Succeeded, road.Failure);
            Assert.NotNull(road.Vertices);
            Assert.All(road.Vertices, point => Assert.True(point.GroundZ > 10));
            Assert.Contains(
                road.Vertices,
                point => point.X > 11 && point.X < 14 && (point.Y < 6 || point.Y > 11));
        }

        [Fact]
        public async Task DesignedRoad_ClimbsSteeperSlopeByTraversing()
        {
            using var tmp = new TempLas();
            var points = new List<byte[]>();

            for (int x = 0; x <= 60; x++)
            {
                for (int y = 0; y <= 80; y++)
                {
                    if (x % 8 != 0 || y % 8 != 0)
                        continue;

                    points.Add(PointFormat0(
                        Enc(x + 0.5, 0.001, 0),
                        Enc(y + 0.5, 0.001, 0),
                        Enc(x * 0.14, 0.001, 0),
                        2,
                        false,
                        20));
                }
            }

            byte[] header = CreateHeader(
                minor: 2,
                headerSize: 227,
                format: 0,
                recordLength: 20,
                legacyCount: (uint)points.Count,
                extendedCount: 0,
                pointOffset: 227,
                vlrCount: 0,
                scaleX: 0.001, scaleY: 0.001, scaleZ: 0.001,
                offX: 0, offY: 0, offZ: 0,
                minX: 0, maxX: 62, minY: 0, maxY: 82, minZ: 0, maxZ: 20);
            Save(tmp.Path, header, null, points);

            ElevationGrid grid = await ElevationGrid.BuildAsync(tmp.Path);
            DesignedRoadSearchResult road = grid.FindDesignedRoad(
                0.5, 40.5, 48.5, 40.5, 1, 9, 3);

            Assert.True(road.Succeeded, road.Failure);
            Assert.NotNull(road.Vertices);
            double rise = road.Vertices[^1].RoadZ - road.Vertices[0].RoadZ;
            Assert.InRange(rise, 6.4, 7.0);
            Assert.True(RoadProfile.GetMaximumGradePercent(road.Vertices) <= 9.2);
        }

        private static byte[] CreateHeader(
            byte minor,
            ushort headerSize,
            byte format,
            ushort recordLength,
            uint legacyCount,
            ulong extendedCount,
            uint pointOffset,
            uint vlrCount,
            double scaleX,
            double scaleY,
            double scaleZ,
            double offX,
            double offY,
            double offZ,
            double minX,
            double maxX,
            double minY,
            double maxY,
            double minZ,
            double maxZ)
        {
            var header = new byte[headerSize];
            header[0] = (byte)'L';
            header[1] = (byte)'A';
            header[2] = (byte)'S';
            header[3] = (byte)'F';
            header[24] = 1;
            header[25] = minor;
            Encoding.ASCII.GetBytes("unit-test").CopyTo(header.AsSpan(58));
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(94), headerSize);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(96), pointOffset);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(100), vlrCount);
            header[104] = format;
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(105), recordLength);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(107), legacyCount);
            WriteDouble(header, 131, scaleX);
            WriteDouble(header, 139, scaleY);
            WriteDouble(header, 147, scaleZ);
            WriteDouble(header, 155, offX);
            WriteDouble(header, 163, offY);
            WriteDouble(header, 171, offZ);
            WriteDouble(header, 179, maxX);
            WriteDouble(header, 187, minX);
            WriteDouble(header, 195, maxY);
            WriteDouble(header, 203, minY);
            WriteDouble(header, 211, maxZ);
            WriteDouble(header, 219, minZ);
            if (headerSize >= 255)
                BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(247), extendedCount);
            return header;
        }

        private static void WriteDouble(byte[] buffer, int offset, double value) =>
            BinaryPrimitives.WriteDoubleLittleEndian(buffer.AsSpan(offset, 8), value);

        private static byte[] PointFormat0(int x, int y, int z, byte classification, bool withheld, int recordLength)
        {
            var point = new byte[recordLength];
            BinaryPrimitives.WriteInt32LittleEndian(point.AsSpan(0, 4), x);
            BinaryPrimitives.WriteInt32LittleEndian(point.AsSpan(4, 4), y);
            BinaryPrimitives.WriteInt32LittleEndian(point.AsSpan(8, 4), z);
            byte raw = (byte)(classification & 0x1F);
            if (withheld)
                raw |= 0x80;
            point[15] = raw;
            for (int i = 20; i < recordLength; i++)
                point[i] = 0xFF;
            return point;
        }

        private static byte[] PointFormat6(int x, int y, int z, byte classification, byte flags)
        {
            var point = new byte[30];
            BinaryPrimitives.WriteInt32LittleEndian(point.AsSpan(0, 4), x);
            BinaryPrimitives.WriteInt32LittleEndian(point.AsSpan(4, 4), y);
            BinaryPrimitives.WriteInt32LittleEndian(point.AsSpan(8, 4), z);
            point[15] = flags;
            point[16] = classification;
            return point;
        }

        private static byte[] CreateVlr(string userId, ushort recordId, byte[] data)
        {
            var vlr = new byte[54 + data.Length];
            Encoding.ASCII.GetBytes(userId).AsSpan(0, Math.Min(16, userId.Length)).CopyTo(vlr.AsSpan(2));
            BinaryPrimitives.WriteUInt16LittleEndian(vlr.AsSpan(18), recordId);
            BinaryPrimitives.WriteUInt16LittleEndian(vlr.AsSpan(20), (ushort)data.Length);
            data.CopyTo(vlr.AsSpan(54));
            return vlr;
        }

        private static int Enc(double value, double scale, double offset) =>
            (int)Math.Round((value - offset) / scale, MidpointRounding.AwayFromZero);

        private static void Save(string path, byte[] header, byte[]? vlr, IEnumerable<byte[]> points)
        {
            using var stream = File.Create(path);
            stream.Write(header);
            if (vlr != null)
                stream.Write(vlr);
            foreach (byte[] point in points)
                stream.Write(point);
        }

        private sealed class TempLas : IDisposable
        {
            public TempLas(string extension = ".las")
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "find_forestLoad_" + Guid.NewGuid().ToString("N") + extension);
            }

            public string Path { get; }

            public void Dispose()
            {
                try
                {
                    if (File.Exists(Path))
                        File.Delete(Path);
                }
                catch (IOException)
                {
                }
            }
        }
    }
}
