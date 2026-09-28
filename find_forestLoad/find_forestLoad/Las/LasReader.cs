using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace find_forestLoad.Las
{
    internal interface ILasPointConsumer
    {
        void OnPoint(double x, double y, double z, byte classification, bool withheld);
    }

    internal static class LasReader
    {
        public static LasHeader ReadHeader(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("LAS 파일 경로가 비어 있습니다.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("LAS 파일을 찾을 수 없습니다.", path);

            if (path.EndsWith(".laz", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("압축된 LAZ(LASzip)는 읽을 수 없습니다. 압축을 푼 LAS로 저장한 뒤 다시 여세요.");

            using var stream = Open(path);
            var preamble = new byte[227];
            ReadExactlyOrThrow(stream, preamble);

            if (preamble[0] != (byte)'L' || preamble[1] != (byte)'A' || preamble[2] != (byte)'S' || preamble[3] != (byte)'F')
                throw new InvalidDataException("LAS 파일이 아닙니다.");

            int versionMajor = preamble[24];
            int versionMinor = preamble[25];
            if (versionMajor != 1)
                throw new InvalidDataException("LAS 1.x 파일만 읽을 수 있습니다.");

            int headerSize = BinaryPrimitives.ReadUInt16LittleEndian(preamble.AsSpan(94));
            if (headerSize < 227 || headerSize > 65_536)
                throw new InvalidDataException("헤더 크기가 올바르지 않습니다.");

            byte[] header = preamble;
            if (headerSize > 227)
            {
                header = new byte[headerSize];
                preamble.AsSpan().CopyTo(header);
                ReadExactlyOrThrow(stream, header.AsSpan(227));
            }

            long pointOffset = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(96));
            if (pointOffset < headerSize || pointOffset > stream.Length)
                throw new InvalidDataException("점 데이터 시작 위치가 잘못되었습니다.");

            int format = header[104];
            int recordLength = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(105));
            int minimumLength = MinimumRecordLength(format);
            if (recordLength < minimumLength)
                throw new InvalidDataException($"점 레코드 길이가 너무 짧습니다. 형식 {format}은 {minimumLength}바이트 이상이어야 합니다.");

            long pointCount = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(107));
            if (versionMinor >= 4 && header.Length >= 255)
            {
                ulong extendedCount = BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(247));
                if (extendedCount > 0)
                {
                    if (extendedCount > long.MaxValue)
                        throw new InvalidDataException("점 개수가 너무 큽니다.");
                    pointCount = (long)extendedCount;
                }
            }

            if (pointCount <= 0)
                throw new InvalidDataException("점 개수가 0입니다.");

            if (pointCount > (long.MaxValue - pointOffset) / recordLength)
                throw new InvalidDataException("점 개수가 너무 큽니다.");

            if (pointOffset + pointCount * recordLength > stream.Length)
                throw new InvalidDataException("파일 크기가 헤더의 점 개수보다 작습니다.");

            double scaleX = ReadDouble(header, 131);
            double scaleY = ReadDouble(header, 139);
            double scaleZ = ReadDouble(header, 147);
            if (scaleX == 0 || scaleY == 0 || scaleZ == 0)
                throw new InvalidDataException("좌표 축척(scale)이 0입니다.");

            double minX = ReadDouble(header, 187);
            double maxX = ReadDouble(header, 179);
            double minY = ReadDouble(header, 203);
            double maxY = ReadDouble(header, 195);
            double minZ = ReadDouble(header, 219);
            double maxZ = ReadDouble(header, 211);
            if (minX > maxX || minY > maxY || minZ > maxZ)
                throw new InvalidDataException("헤더의 최소 좌표가 최대 좌표보다 큽니다.");

            long vlrCount = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(100));
            if (vlrCount > 10_000)
                throw new InvalidDataException("가변 길이 레코드 개수가 너무 많습니다.");

            string? crs = null;
            for (long i = 0; i < vlrCount; i++)
            {
                if (stream.Position + 54 > pointOffset)
                    throw new InvalidDataException("가변 길이 레코드가 점 데이터 위치를 넘습니다.");

                var vlr = new byte[54];
                ReadExactlyOrThrow(stream, vlr);
                string userId = ReadAscii(vlr, 2, 16);
                ushort recordId = BinaryPrimitives.ReadUInt16LittleEndian(vlr.AsSpan(18));
                int dataLength = BinaryPrimitives.ReadUInt16LittleEndian(vlr.AsSpan(20));
                if (stream.Position + dataLength > pointOffset)
                    throw new InvalidDataException("가변 길이 레코드가 점 데이터 영역을 침범합니다.");

                var data = dataLength == 0 ? [] : new byte[dataLength];
                if (dataLength > 0)
                    ReadExactlyOrThrow(stream, data);

                if (userId.Contains("laszip", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("압축된 LAZ(LASzip)는 읽을 수 없습니다. 압축을 푼 LAS로 저장한 뒤 다시 여세요.");

                if (recordId == 2112 && crs == null)
                    crs = DecodeCrs(data);
            }

            return new LasHeader
            {
                VersionMajor = versionMajor,
                VersionMinor = versionMinor,
                PointDataFormat = format,
                PointDataRecordLength = recordLength,
                PointCount = pointCount,
                ScaleX = scaleX,
                ScaleY = scaleY,
                ScaleZ = scaleZ,
                OffsetX = ReadDouble(header, 155),
                OffsetY = ReadDouble(header, 163),
                OffsetZ = ReadDouble(header, 171),
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY,
                MinZ = minZ,
                MaxZ = maxZ,
                HeaderSize = headerSize,
                PointDataOffset = pointOffset,
                GeneratingSoftware = ReadAscii(header, 58, 32),
                CrsWkt = crs
            };
        }

        internal static void ReadPoints<TConsumer>(
            Stream stream,
            LasHeader header,
            long firstPoint,
            long pointCount,
            ref TConsumer consumer,
            string? phase,
            IProgress<ElevationBuildProgress>? progress,
            CancellationToken cancellationToken)
            where TConsumer : ILasPointConsumer
        {
            if (firstPoint < 0 || pointCount < 0 || firstPoint > header.PointCount - pointCount)
                throw new ArgumentOutOfRangeException(nameof(pointCount));

            int recordLength = header.PointDataRecordLength;
            long position = header.PointDataOffset + firstPoint * recordLength;
            stream.Position = position;

            int chunkPoints = Math.Max(1, (1 << 20) / recordLength);
            var buffer = new byte[chunkPoints * recordLength];
            double scaleX = header.ScaleX;
            double scaleY = header.ScaleY;
            double scaleZ = header.ScaleZ;
            double offsetX = header.OffsetX;
            double offsetY = header.OffsetY;
            double offsetZ = header.OffsetZ;
            int format = header.PointDataFormat;
            long remaining = pointCount;
            long done = 0;
            int lastPercent = -1;

            while (remaining > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int wantPoints = (int)Math.Min(remaining, chunkPoints);
                int wantBytes = wantPoints * recordLength;
                ReadExactlyOrThrow(stream, buffer.AsSpan(0, wantBytes));

                for (int i = 0; i < wantPoints; i++)
                {
                    var record = buffer.AsSpan(i * recordLength, recordLength);
                    int xi = BinaryPrimitives.ReadInt32LittleEndian(record);
                    int yi = BinaryPrimitives.ReadInt32LittleEndian(record.Slice(4, 4));
                    int zi = BinaryPrimitives.ReadInt32LittleEndian(record.Slice(8, 4));
                    var (classification, withheld) = ReadClassification(format, record);
                    consumer.OnPoint(
                        xi * scaleX + offsetX,
                        yi * scaleY + offsetY,
                        zi * scaleZ + offsetZ,
                        classification,
                        withheld);
                }

                remaining -= wantPoints;
                done += wantPoints;
                if (progress != null && phase != null && pointCount > 0)
                {
                    int percent = (int)(done * 100 / pointCount);
                    if (percent != lastPercent)
                    {
                        lastPercent = percent;
                        progress.Report(new ElevationBuildProgress(done / (double)pointCount, $"{phase}... {percent}%"));
                    }
                }
            }
        }

        internal static FileStream Open(string path)
        {
            try
            {
                return new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    1 << 20,
                    FileOptions.SequentialScan);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new IOException("LAS 파일을 열 수 없습니다. " + ex.Message, ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (byte Classification, bool Withheld) ReadClassification(int format, ReadOnlySpan<byte> point)
        {
            // 형식 0–5: 분류는 15번 바이트의 하위 5비트, 보류는 최상위 비트.
            // 형식 6–10: 분류는 16번 바이트, 보류는 15번 바이트의 비트 2.
            if (format <= 5)
            {
                byte raw = point[15];
                return ((byte)(raw & 0x1F), (raw & 0x80) != 0);
            }

            return (point[16], (point[15] & 0x04) != 0);
        }

        private static int MinimumRecordLength(int format) => format switch
        {
            0 => 20,
            1 => 28,
            2 => 26,
            3 => 34,
            4 => 57,
            5 => 63,
            6 => 30,
            7 => 36,
            8 => 38,
            9 => 59,
            10 => 67,
            _ => throw new InvalidDataException($"지원하지 않는 점 형식입니다. (형식 {format})")
        };

        private static void ReadExactlyOrThrow(Stream stream, Span<byte> buffer)
        {
            try
            {
                stream.ReadExactly(buffer);
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException("LAS 파일이 잘렸습니다.");
            }
        }

        private static double ReadDouble(byte[] header, int offset)
        {
            double value = BinaryPrimitives.ReadDoubleLittleEndian(header.AsSpan(offset, 8));
            if (!double.IsFinite(value))
                throw new InvalidDataException("헤더 좌표 값이 올바르지 않습니다.");
            return value;
        }

        private static string ReadAscii(byte[] bytes, int offset, int length)
        {
            int end = offset;
            int stop = Math.Min(bytes.Length, offset + length);
            while (end < stop && bytes[end] != 0)
                end++;
            return Encoding.ASCII.GetString(bytes, offset, end - offset).Trim();
        }

        private static string DecodeCrs(byte[] data)
        {
            int end = Array.IndexOf(data, (byte)0);
            if (end < 0)
                end = data.Length;
            return Encoding.UTF8.GetString(data, 0, end).Trim();
        }
    }
}
