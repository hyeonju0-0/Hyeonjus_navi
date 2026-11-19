using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using find_forestLoad.Las;

namespace find_forestLoad;

internal static class RoadObjectiveStyle
{
    public static string Caption(RoadObjective objective) => objective switch
    {
        RoadObjective.Safety => "안정성",
        RoadObjective.Cost => "비용 추정",
        RoadObjective.Distance => "거리 후보",
        RoadObjective.ConstructionTime => "공사 기간 추정",
        _ => objective.ToString()
    };

    public static Color ColorOf(RoadObjective objective) => objective switch
    {
        RoadObjective.Safety => Color.FromArgb(198, 40, 40),
        RoadObjective.Cost => Color.FromArgb(176, 132, 0),
        RoadObjective.Distance => Color.FromArgb(0, 138, 176),
        RoadObjective.ConstructionTime => Color.FromArgb(156, 40, 156),
        _ => Color.Black
    };
}

internal static class TerrainColor
{
    private static readonly (float T, byte R, byte G, byte B)[] Stops =
    [
        (0.00f, 28, 72, 48),
        (0.22f, 52, 128, 68),
        (0.42f, 138, 164, 72),
        (0.62f, 196, 164, 86),
        (0.80f, 154, 102, 64),
        (1.00f, 244, 240, 230)
    ];

    public static Color Ramp(float t)
    {
        (byte r, byte g, byte b) = Rgb(t);
        return Color.FromArgb(r, g, b);
    }

    public static Bitmap Colorize(
        float[] samples,
        int width,
        int height,
        float minZ,
        float maxZ,
        double metersX,
        double metersY)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        Rectangle rect = new(0, 0, width, height);
        BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            int stride = data.Stride;
            var row = new byte[width * 4];
            float span = maxZ - minZ;
            for (int y = 0; y < height; y++)
            {
                int source = y * width;
                for (int x = 0; x < width; x++)
                {
                    float z = samples[source + x];
                    int pixel = x * 4;
                    if (float.IsNaN(z))
                    {
                        row[pixel] = 48;
                        row[pixel + 1] = 52;
                        row[pixel + 2] = 56;
                        row[pixel + 3] = 255;
                        continue;
                    }

                    float t = span <= 0 ? 0.5f : (z - minZ) / span;
                    (byte r, byte g, byte b) = Rgb(t);
                    float shade = Shade(samples, width, height, x, y, metersX, metersY);
                    row[pixel] = Scale(b, shade);
                    row[pixel + 1] = Scale(g, shade);
                    row[pixel + 2] = Scale(r, shade);
                    row[pixel + 3] = 255;
                }

                Marshal.Copy(row, 0, data.Scan0 + y * stride, row.Length);
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return bitmap;
    }

    private static (byte R, byte G, byte B) Rgb(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        for (int i = 1; i < Stops.Length; i++)
        {
            if (t > Stops[i].T)
                continue;

            float from = Stops[i - 1].T;
            float to = Stops[i].T;
            float u = to <= from ? 0 : (t - from) / (to - from);
            return (
                Lerp(Stops[i - 1].R, Stops[i].R, u),
                Lerp(Stops[i - 1].G, Stops[i].G, u),
                Lerp(Stops[i - 1].B, Stops[i].B, u));
        }

        var last = Stops[^1];
        return (last.R, last.G, last.B);
    }

    private static byte Lerp(byte a, byte b, float u) =>
        (byte)Math.Clamp(Math.Round(a + (b - a) * u), 0, 255);

    private static byte Scale(byte channel, float shade) =>
        (byte)Math.Clamp(Math.Round(channel * shade), 0, 255);

    private static float Shade(
        float[] samples,
        int width,
        int height,
        int x,
        int y,
        double metersX,
        double metersY)
    {
        if (metersX <= 0 || metersY <= 0)
            return 1;

        float z = samples[y * width + x];
        float dzdx = 0;
        float dzdy = 0;
        if (TryAt(samples, width, height, x - 1, y, out float left)
            && TryAt(samples, width, height, x + 1, y, out float right))
            dzdx = (right - left) / (float)(2 * metersX);
        else if (TryAt(samples, width, height, x - 1, y, out left))
            dzdx = (z - left) / (float)metersX;
        else if (TryAt(samples, width, height, x + 1, y, out right))
            dzdx = (right - z) / (float)metersX;

        if (TryAt(samples, width, height, x, y - 1, out float north)
            && TryAt(samples, width, height, x, y + 1, out float south))
            dzdy = (north - south) / (float)(2 * metersY);
        else if (TryAt(samples, width, height, x, y - 1, out north))
            dzdy = (north - z) / (float)metersY;
        else if (TryAt(samples, width, height, x, y + 1, out south))
            dzdy = (z - south) / (float)metersY;

        float nx = -dzdx;
        float ny = -dzdy;
        float nz = 1f;
        float nlen = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
        const float lx = -0.55f;
        const float ly = 0.45f;
        const float lz = 0.70f;
        float llen = MathF.Sqrt(lx * lx + ly * ly + lz * lz);
        float light = (nx / nlen * lx + ny / nlen * ly + nz / nlen * lz) / llen;
        if (light < 0)
            light = 0;
        return 0.42f + 0.58f * light;
    }

    private static bool TryAt(float[] samples, int width, int height, int x, int y, out float z)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height)
        {
            z = 0;
            return false;
        }

        z = samples[y * width + x];
        return !float.IsNaN(z);
    }
}
