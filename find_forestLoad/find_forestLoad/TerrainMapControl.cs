using System.Drawing.Drawing2D;
using System.Globalization;
using find_forestLoad.Las;

namespace find_forestLoad;

internal sealed class TerrainMapControl : Control
{
    public const string Hint = "휠 확대·축소 · 드래그 이동 · 더블 클릭으로 전체 보기";
    private const long DetailCellLimit = 4_000_000;

    private ElevationGrid? _grid;
    private TerrainOverview _overview;
    private Bitmap? _overviewBitmap;
    private Bitmap? _detailBitmap;
    private float[]? _samples;
    private double _centerX;
    private double _centerY;
    private double _metersPerPixel = 1;
    private bool _fitPending = true;
    private bool _userNavigated;
    private bool _pressing;
    private Point _pressPoint;
    private bool _dragging;
    private Point _dragLast;
    private int _dragOffsetX;
    private int _dragOffsetY;
    private bool _detailDrag;
    private double _detailCenterX;
    private double _detailCenterY;
    private double _detailMeters;
    private Size _detailSize;

    private readonly TerrainPreviewPanel _view;

    public TerrainMapControl(TerrainPreviewPanel view)
    {
        _view = view;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
        BackColor = Color.FromArgb(36, 40, 44);
        TabStop = true;
    }

    public event Action<string>? HoverTextChanged;

    public event Action? ViewChanged;

    public bool PointerInside { get; private set; }

    public void SetContent(ElevationGrid grid, TerrainOverview overview, Bitmap overviewBitmap, bool keepView)
    {
        _grid = grid;
        _overview = overview;
        _overviewBitmap = overviewBitmap;
        _detailBitmap?.Dispose();
        _detailBitmap = null;
        if (!keepView)
            _fitPending = true;
        if (IsHandleCreated && _fitPending)
            Fit();
        else
            Invalidate();
    }

    public void Clear()
    {
        _grid = null;
        _overviewBitmap = null;
        _detailBitmap?.Dispose();
        _detailBitmap = null;
        _samples = null;
        _fitPending = true;
        Invalidate();
    }

    public void Fit()
    {
        if (_grid == null)
            return;

        if (ClientSize.Width < 2 || ClientSize.Height < 2)
        {
            _fitPending = true;
            return;
        }

        double minX = _overview.FilledMinX;
        double minY = _overview.FilledMinY;
        double maxX = _overview.FilledMaxX;
        double maxY = _overview.FilledMaxY;
        double spanX = Math.Max(maxX - minX, _grid.CellSizeMeters);
        double spanY = Math.Max(maxY - minY, _grid.CellSizeMeters);
        double padX = Math.Max(spanX * 0.08, _grid.CellSizeMeters * 2);
        double padY = Math.Max(spanY * 0.08, _grid.CellSizeMeters * 2);
        spanX += padX * 2;
        spanY += padY * 2;
        _centerX = (minX + maxX) / 2;
        _centerY = (minY + maxY) / 2;
        _metersPerPixel = Math.Max(spanX / ClientSize.Width, spanY / ClientSize.Height);
        _fitPending = false;
        _userNavigated = false;
        Invalidate();
    }

    internal bool TryScreenToWorld(Point point, out double x, out double y)
    {
        if (_grid == null || ClientSize.Width < 2 || ClientSize.Height < 2)
        {
            x = 0;
            y = 0;
            return false;
        }

        ScreenToWorld(point, out x, out y);
        return true;
    }

    internal void FitToRoutes(RoadObjective? focus)
    {
        if (_grid == null || ClientSize.Width < 2 || ClientSize.Height < 2)
            return;

        double minX = double.PositiveInfinity;
        double minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity;
        double maxY = double.NegativeInfinity;
        int count = 0;

        foreach ((RoadObjective objective, IReadOnlyList<RoadDesignVertex> profile) in _view.Profiles)
        {
            if (focus is RoadObjective one && objective != one)
                continue;
            if (focus == null && !_view.Shows(objective))
                continue;
            if (profile.Count < 2)
                continue;

            foreach (RoadDesignVertex point in profile)
            {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
                count++;
            }
        }

        if (count == 0)
            return;

        double spanX = Math.Max(maxX - minX, _grid.CellSizeMeters);
        double spanY = Math.Max(maxY - minY, _grid.CellSizeMeters);
        double padX = Math.Max(spanX * 0.12, _grid.CellSizeMeters * 4);
        double padY = Math.Max(spanY * 0.12, _grid.CellSizeMeters * 4);
        _centerX = (minX + maxX) / 2;
        _centerY = (minY + maxY) / 2;
        _metersPerPixel = Math.Max((spanX + padX * 2) / ClientSize.Width, (spanY + padY * 2) / ClientSize.Height);
        _fitPending = false;
        _userNavigated = true;
        Invalidate();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (_fitPending)
            Fit();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (ClientSize.Width < 2 || ClientSize.Height < 2)
        {
            _fitPending = true;
            return;
        }

        if (_fitPending || !_userNavigated)
            Fit();
        else
            Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (e.Button != MouseButtons.Left || _grid == null)
            return;

        _pressing = true;
        _pressPoint = e.Location;
        _dragging = false;
        _dragOffsetX = 0;
        _dragOffsetY = 0;
        Capture = true;
        Cursor = _view.IsPicking ? Cursors.Cross : Cursors.SizeAll;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        PointerInside = true;
        if (_pressing && !_dragging)
        {
            int sx = e.X - _pressPoint.X;
            int sy = e.Y - _pressPoint.Y;
            if (sx * sx + sy * sy < 16)
            {
                PublishHover(e.Location);
                return;
            }

            _dragging = true;
            _userNavigated = true;
            _dragLast = _pressPoint;
            _detailDrag = !UseOverview && _detailBitmap != null;
            Cursor = Cursors.SizeAll;
        }

        if (_dragging)
        {
            int dx = e.X - _dragLast.X;
            int dy = e.Y - _dragLast.Y;
            _dragLast = e.Location;
            _centerX -= dx * _metersPerPixel;
            _centerY += dy * _metersPerPixel;
            _dragOffsetX += dx;
            _dragOffsetY += dy;
            Invalidate();
        }

        PublishHover(e.Location);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        bool click = _pressing && !_dragging && e.Button == MouseButtons.Left;
        _pressing = false;
        _dragging = false;
        _detailDrag = false;
        _dragOffsetX = 0;
        _dragOffsetY = 0;
        Capture = false;
        Cursor = Cursors.Cross;
        if (click)
            _view.AcceptMapClick(e.Location);
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        PointerInside = true;
        Cursor = Cursors.Cross;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        PointerInside = false;
        if (!_dragging)
            Cursor = Cursors.Default;
        ViewChanged?.Invoke();
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        if (e.Button == MouseButtons.Left)
            Fit();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_grid == null || ClientSize.Width < 2)
            return;

        _userNavigated = true;
        ScreenToWorld(e.Location, out double worldX, out double worldY);
        double factor = e.Delta > 0 ? 1 / 1.25 : 1.25;
        double next = Math.Clamp(factor * _metersPerPixel, MinMetersPerPixel(), MaxMetersPerPixel());
        _centerX = worldX - (e.X - ClientSize.Width / 2.0) * next;
        _centerY = worldY + (e.Y - ClientSize.Height / 2.0) * next;
        _metersPerPixel = next;
        _dragging = false;
        _detailDrag = false;
        Invalidate();
        PublishHover(e.Location);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);
        if (_grid == null || _overviewBitmap == null)
            return;

        g.InterpolationMode = InterpolationMode.Bilinear;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        if (UseOverview)
            DrawOverview(g);
        else
            DrawDetail(g);

        DrawRoutes(g);
        DrawMarkers(g);
        DrawScale(g);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _detailBitmap?.Dispose();
        base.Dispose(disposing);
    }

    private bool UseOverview
    {
        get
        {
            if (_grid == null || ClientSize.Width < 2 || ClientSize.Height < 2)
                return true;

            ViewWorld(out double minX, out double minY, out double maxX, out double maxY);
            long cells = CountCells(minX, minY, maxX, maxY);
            return cells > DetailCellLimit;
        }
    }

    private void DrawOverview(Graphics g)
    {
        if (_grid == null || _overviewBitmap == null)
            return;

        ViewWorld(out double minX, out double minY, out double maxX, out double maxY);
        double visMinX = Math.Max(minX, _grid.GridMinX);
        double visMaxX = Math.Min(maxX, _grid.GridMaxX);
        double visMinY = Math.Max(minY, _grid.GridMinY);
        double visMaxY = Math.Min(maxY, _grid.GridMaxY);
        if (visMaxX <= visMinX || visMaxY <= visMinY)
            return;

        double gridW = _grid.GridMaxX - _grid.GridMinX;
        double gridH = _grid.GridMaxY - _grid.GridMinY;
        if (gridW <= 0 || gridH <= 0)
            return;

        float destLeft = WorldToScreenX(visMinX);
        float destRight = WorldToScreenX(visMaxX);
        float destTop = WorldToScreenY(visMaxY);
        float destBottom = WorldToScreenY(visMinY);
        float srcLeft = (float)((visMinX - _grid.GridMinX) / gridW * _overview.Width);
        float srcRight = (float)((visMaxX - _grid.GridMinX) / gridW * _overview.Width);
        float srcTop = (float)((_grid.GridMaxY - visMaxY) / gridH * _overview.Height);
        float srcBottom = (float)((_grid.GridMaxY - visMinY) / gridH * _overview.Height);
        g.DrawImage(
            _overviewBitmap,
            new RectangleF(destLeft, destTop, destRight - destLeft, destBottom - destTop),
            new RectangleF(srcLeft, srcTop, srcRight - srcLeft, srcBottom - srcTop),
            GraphicsUnit.Pixel);
    }

    private void DrawDetail(Graphics g)
    {
        if (_detailDrag && _detailBitmap != null)
        {
            g.DrawImage(_detailBitmap, _dragOffsetX, _dragOffsetY);
            return;
        }

        EnsureDetail();
        if (_detailBitmap != null)
            g.DrawImage(_detailBitmap, 0, 0);
    }

    private void EnsureDetail()
    {
        if (_grid == null || ClientSize.Width < 2 || ClientSize.Height < 2)
            return;

        if (_detailBitmap != null
            && _detailSize == ClientSize
            && _detailCenterX == _centerX
            && _detailCenterY == _centerY
            && _detailMeters == _metersPerPixel)
            return;

        int width = ClientSize.Width;
        int height = ClientSize.Height;
        int count = width * height;
        if (_samples == null || _samples.Length != count)
            _samples = new float[count];

        ViewWorld(out double minX, out double minY, out double maxX, out double maxY);
        _grid.SampleWindow(minX, minY, maxX, maxY, width, height, _samples);
        Bitmap bitmap = TerrainColor.Colorize(
            _samples,
            width,
            height,
            _overview.MinZ,
            _overview.MaxZ,
            _metersPerPixel,
            _metersPerPixel);
        _detailBitmap?.Dispose();
        _detailBitmap = bitmap;
        _detailSize = ClientSize;
        _detailCenterX = _centerX;
        _detailCenterY = _centerY;
        _detailMeters = _metersPerPixel;
    }

    private void DrawRoutes(Graphics g)
    {
        TerrainPreviewPanel view = _view;
        RoadObjective? emphasis = view.Emphasized;
        foreach (RoadObjective objective in Enum.GetValues<RoadObjective>())
        {
            if (objective == emphasis)
                continue;
            DrawOneRoute(g, view, objective, emphasized: false);
        }

        if (emphasis is RoadObjective selected)
        {
            DrawOneRoute(g, view, selected, emphasized: true);
            if (view.Profiles.TryGetValue(selected, out IReadOnlyList<RoadDesignVertex>? profile))
                DrawStations(g, profile);
        }
    }

    private void DrawOneRoute(Graphics g, TerrainPreviewPanel view, RoadObjective objective, bool emphasized)
    {
        if (!view.Shows(objective)
            || !view.Profiles.TryGetValue(objective, out IReadOnlyList<RoadDesignVertex>? profile)
            || profile.Count < 2)
            return;

        float width = emphasized ? 3.4f : view.Emphasized == null ? 2.6f : 1.7f;
        using var halo = new Pen(Color.FromArgb(210, 20, 20, 20), width + 2.2f)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using var pen = new Pen(RoadObjectiveStyle.ColorOf(objective), width)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        DrawPolyline(g, halo, profile);
        DrawPolyline(g, pen, profile);
    }

    private void DrawStations(Graphics g, IReadOnlyList<RoadDesignVertex> profile)
    {
        using var font = new Font(Font.FontFamily, 8f, FontStyle.Bold);
        using var fill = new SolidBrush(Color.White);
        using var ring = new Pen(Color.FromArgb(40, 40, 40), 1.4f);
        using var text = new SolidBrush(Color.FromArgb(30, 30, 30));
        PointF? previous = null;

        for (int i = 0; i < profile.Count; i++)
        {
            PointF point = WorldToScreen(profile[i].X, profile[i].Y);
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                continue;
            if (point.X < -20 || point.Y < -20 || point.X > ClientSize.Width + 20 || point.Y > ClientSize.Height + 20)
                continue;

            bool endpoint = i == 0 || i == profile.Count - 1;
            if (!endpoint && previous is PointF last)
            {
                float dx = point.X - last.X;
                float dy = point.Y - last.Y;
                if (dx * dx + dy * dy < 32 * 32)
                    continue;
            }

            var disk = new RectangleF(point.X - 4f, point.Y - 4f, 8f, 8f);
            g.FillEllipse(fill, disk);
            g.DrawEllipse(ring, disk);
            string label = "P" + (i + 1).ToString(CultureInfo.InvariantCulture);
            g.DrawString(label, font, text, point.X + 5f, point.Y - 12f);
            previous = point;
        }
    }

    private void DrawPolyline(Graphics g, Pen pen, IReadOnlyList<RoadDesignVertex> profile)
    {
        var clip = new RectangleF(-32, -32, ClientSize.Width + 64, ClientSize.Height + 64);
        PointF previous = WorldToScreen(profile[0].X, profile[0].Y);
        for (int i = 1; i < profile.Count; i++)
        {
            PointF next = WorldToScreen(profile[i].X, profile[i].Y);
            float x0 = previous.X;
            float y0 = previous.Y;
            float x1 = next.X;
            float y1 = next.Y;
            previous = next;
            if (!float.IsFinite(x0) || !float.IsFinite(y0) || !float.IsFinite(x1) || !float.IsFinite(y1))
                continue;
            if (!ClipSegment(ref x0, ref y0, ref x1, ref y1, clip))
                continue;

            g.DrawLine(pen, x0, y0, x1, y1);
        }
    }

    private void DrawMarkers(Graphics g)
    {
        if (!_view.ShowsMarkers)
            return;

        TerrainPreviewPanel view = _view;

        if (view.Start is { } start)
            DrawPin(g, start.X, start.Y, Color.FromArgb(198, 40, 40), "시점");
        if (view.End is { } end)
            DrawPin(g, end.X, end.Y, Color.FromArgb(25, 85, 180), "종점");
    }

    private void DrawPin(Graphics g, double x, double y, Color color, string label)
    {
        PointF point = WorldToScreen(x, y);
        if (point.X < -30 || point.Y < -30 || point.X > ClientSize.Width + 30 || point.Y > ClientSize.Height + 30)
            return;

        var disk = new RectangleF(point.X - 6.5f, point.Y - 6.5f, 13, 13);
        using var fill = new SolidBrush(Color.White);
        using var ring = new Pen(color, 2.4f);
        g.FillEllipse(fill, disk);
        g.DrawEllipse(ring, disk);

        SizeF size = g.MeasureString(label, Font);
        bool placeLeft = point.X + 14 + size.Width > ClientSize.Width - 8;
        float tx = placeLeft ? point.X - 10 - size.Width : point.X + 10;
        float ty = point.Y - size.Height / 2f;
        using var back = new SolidBrush(Color.FromArgb(230, 255, 255, 255));
        g.FillRectangle(back, tx - 2, ty - 1, size.Width + 4, size.Height + 2);
        using var text = new SolidBrush(color);
        g.DrawString(label, Font, text, tx, ty);
    }

    private void DrawScale(Graphics g)
    {
        if (_metersPerPixel <= 0 || ClientSize.Width < 80)
            return;

        double meters = NiceLength(120 * _metersPerPixel);
        float pixels = (float)(meters / _metersPerPixel);
        if (pixels < 24 || pixels > ClientSize.Width - 40)
            return;

        float x = 16;
        float y = ClientSize.Height - 22;
        using var back = new SolidBrush(Color.FromArgb(170, 20, 20, 20));
        string text = meters >= 1000
            ? (meters / 1000).ToString("0.##", CultureInfo.CurrentCulture) + " km"
            : meters.ToString("0.##", CultureInfo.CurrentCulture) + " m";
        SizeF size = g.MeasureString(text, Font);
        g.FillRectangle(back, x - 6, y - size.Height - 8, Math.Max(pixels, size.Width) + 12, size.Height + 18);
        using var pen = new Pen(Color.White, 2f);
        g.DrawLine(pen, x, y, x + pixels, y);
        g.DrawLine(pen, x, y - 5, x, y + 1);
        g.DrawLine(pen, x + pixels, y - 5, x + pixels, y + 1);
        g.DrawString(text, Font, Brushes.White, x, y - size.Height - 2);
    }

    private void PublishHover(Point location)
    {
        if (_grid == null)
            return;

        ScreenToWorld(location, out double x, out double y);
        string text = "X " + x.ToString("0.00", CultureInfo.CurrentCulture)
            + "    Y " + y.ToString("0.00", CultureInfo.CurrentCulture);
        if (_grid.TryGetCellZ(x, y, out double z))
            text += "    고도 " + z.ToString("0.00", CultureInfo.CurrentCulture) + " m";
        else
            text += "    고도 없음";

        HoverTextChanged?.Invoke(text);
    }

    private void ViewWorld(out double minX, out double minY, out double maxX, out double maxY)
    {
        double halfW = ClientSize.Width * 0.5 * _metersPerPixel;
        double halfH = ClientSize.Height * 0.5 * _metersPerPixel;
        minX = _centerX - halfW;
        maxX = _centerX + halfW;
        minY = _centerY - halfH;
        maxY = _centerY + halfH;
    }

    private long CountCells(double minX, double minY, double maxX, double maxY)
    {
        if (_grid == null)
            return 0;

        double cell = _grid.CellSizeMeters;
        int ix0 = (int)Math.Floor((minX - _grid.GridMinX) / cell);
        int ix1 = (int)Math.Floor((maxX - _grid.GridMinX) / cell - 1e-9);
        int iy0 = (int)Math.Floor((minY - _grid.GridMinY) / cell);
        int iy1 = (int)Math.Floor((maxY - _grid.GridMinY) / cell - 1e-9);
        if (ix0 < 0) ix0 = 0;
        if (iy0 < 0) iy0 = 0;
        if (ix1 >= _grid.Width) ix1 = _grid.Width - 1;
        if (iy1 >= _grid.Height) iy1 = _grid.Height - 1;
        if (ix1 < ix0 || iy1 < iy0)
            return 0;

        return (ix1 - ix0 + 1L) * (iy1 - iy0 + 1L);
    }

    private double MinMetersPerPixel() =>
        _grid == null ? 0.05 : _grid.CellSizeMeters / 8;

    private double MaxMetersPerPixel()
    {
        if (_grid == null || ClientSize.Width < 2 || ClientSize.Height < 2)
            return 1000;

        double dataX = Math.Max(_overview.FilledMaxX - _overview.FilledMinX, _grid.CellSizeMeters);
        double dataY = Math.Max(_overview.FilledMaxY - _overview.FilledMinY, _grid.CellSizeMeters);
        double data = Math.Max(dataX / ClientSize.Width, dataY / ClientSize.Height);
        double full = Math.Max(
            (_grid.GridMaxX - _grid.GridMinX) / ClientSize.Width,
            (_grid.GridMaxY - _grid.GridMinY) / ClientSize.Height);
        return Math.Max(data, full) * 1.15;
    }

    private PointF WorldToScreen(double x, double y) =>
        new(WorldToScreenX(x), WorldToScreenY(y));

    private float WorldToScreenX(double x) =>
        (float)((x - _centerX) / _metersPerPixel + ClientSize.Width / 2.0);

    private float WorldToScreenY(double y) =>
        (float)((_centerY - y) / _metersPerPixel + ClientSize.Height / 2.0);

    private void ScreenToWorld(Point point, out double x, out double y)
    {
        x = _centerX + (point.X - ClientSize.Width / 2.0) * _metersPerPixel;
        y = _centerY - (point.Y - ClientSize.Height / 2.0) * _metersPerPixel;
    }

    private static double NiceLength(double meters)
    {
        if (meters <= 0 || double.IsInfinity(meters))
            return 1;

        double exp = Math.Pow(10, Math.Floor(Math.Log10(meters)));
        double fraction = meters / exp;
        double nice = fraction >= 5 ? 5 : fraction >= 2 ? 2 : 1;
        return nice * exp;
    }

    private static bool ClipSegment(
        ref float x0,
        ref float y0,
        ref float x1,
        ref float y1,
        RectangleF rect)
    {
        float dx = x1 - x0;
        float dy = y1 - y0;
        float t0 = 0;
        float t1 = 1;
        if (!ClipSide(-dx, x0 - rect.Left, ref t0, ref t1)) return false;
        if (!ClipSide(dx, rect.Right - x0, ref t0, ref t1)) return false;
        if (!ClipSide(-dy, y0 - rect.Top, ref t0, ref t1)) return false;
        if (!ClipSide(dy, rect.Bottom - y0, ref t0, ref t1)) return false;

        if (t1 < 1)
        {
            x1 = x0 + t1 * dx;
            y1 = y0 + t1 * dy;
        }

        if (t0 > 0)
        {
            x0 += t0 * dx;
            y0 += t0 * dy;
        }

        return true;
    }

    private static bool ClipSide(float p, float q, ref float t0, ref float t1)
    {
        if (Math.Abs(p) < 1e-5f)
            return q >= 0;

        float t = q / p;
        if (p < 0)
        {
            if (t > t1) return false;
            if (t > t0) t0 = t;
        }
        else
        {
            if (t < t0) return false;
            if (t < t1) t1 = t;
        }

        return true;
    }
}
