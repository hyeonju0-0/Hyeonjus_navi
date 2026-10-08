using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace find_forestLoad
{
    internal readonly record struct AutoCadPickResult(
        bool Ok,
        bool Cancelled,
        double X,
        double Y,
        double Z,
        string DrawingName,
        string? Message);

    internal static class AutoCadPointPicker
    {
        private const int World = 0;
        private const int Ucs = 1;
        private const int ModelSpace = 1;

        public static AutoCadPickResult TryPick(string prompt)
        {
            object? appObject = FindRunningAutoCad();
            if (appObject == null)
            {
                bool acadProcess = Process.GetProcessesByName("acad").Length > 0;
                string message = acadProcess
                    ? "AutoCAD는 켜져 있지만 연결하지 못했습니다. 이 프로그램과 AutoCAD를 둘 다 일반 권한으로 다시 실행한 뒤, DWG를 열고 다시 누르세요."
                    : "실행 중인 AutoCAD를 찾지 못했습니다. AutoCAD에서 DWG를 연 뒤 다시 누르세요.";
                return Fail(message);
            }

            try
            {
                dynamic app = appObject;
                app.Visible = true;
                BringToFront(app);

                if (Convert.ToInt32(app.Documents.Count, CultureInfo.InvariantCulture) == 0)
                    return Fail("열린 DWG가 없습니다. AutoCAD에서 도면을 연 뒤 다시 누르세요.");

                dynamic doc = app.ActiveDocument;
                doc.Activate();
                BringToFront(app);

                string drawingName = Convert.ToString(doc.Name) ?? "도면";
                string? spaceNote = null;
                try
                {
                    if (Convert.ToInt32(doc.ActiveSpace, CultureInfo.InvariantCulture) != ModelSpace)
                        spaceNote = "지금 공간은 모형 공간이 아닙니다. 좌표가 배치 기준일 수 있습니다.";
                }
                catch (COMException)
                {
                }

                dynamic utility = doc.Utility;
                object picked;
                try
                {
                    picked = utility.GetPoint(Type.Missing, "\n" + prompt);
                }
                catch (COMException)
                {
                    return new AutoCadPickResult(false, true, 0, 0, 0, drawingName, null);
                }

                object world = picked;
                string? translateNote = null;
                try
                {
                    world = utility.TranslateCoordinates(picked, Ucs, World, false);
                }
                catch (COMException)
                {
                    translateNote = "현재 사용자 좌표계 값을 그대로 넣었습니다.";
                }

                (double x, double y, double z) = ReadXYZ(world);
                string? message = JoinNotes(spaceNote, translateNote);
                return new AutoCadPickResult(true, false, x, y, z, drawingName, message);
            }
            catch (COMException ex)
            {
                return Fail("AutoCAD에서 점을 가져오지 못했습니다. " + ex.Message);
            }
        }

        private static AutoCadPickResult Fail(string message) =>
            new(false, false, 0, 0, 0, "", message);

        private static string? JoinNotes(string? first, string? second)
        {
            if (string.IsNullOrEmpty(first))
                return second;
            if (string.IsNullOrEmpty(second))
                return first;
            return first + " " + second;
        }

        private static (double X, double Y, double Z) ReadXYZ(object point)
        {
            double[] values = point switch
            {
                double[] doubles => doubles,
                float[] floats => Array.ConvertAll(floats, value => (double)value),
                object[] objects => Array.ConvertAll(objects, value => Convert.ToDouble(value, CultureInfo.InvariantCulture)),
                System.Collections.IEnumerable items => items.Cast<object>().Select(value => Convert.ToDouble(value, CultureInfo.InvariantCulture)).ToArray(),
                _ => throw new InvalidOperationException("AutoCAD가 좌표를 반환하지 않았습니다.")
            };

            if (values.Length < 2)
                throw new InvalidOperationException("AutoCAD가 좌표를 반환하지 않았습니다.");

            double z = values.Length >= 3 ? values[2] : 0;
            return (values[0], values[1], z);
        }

        private static void BringToFront(dynamic app)
        {
            try
            {
                long hwnd = Convert.ToInt64(app.HWND, CultureInfo.InvariantCulture);
                if (hwnd != 0)
                    SetForegroundWindow(new IntPtr(hwnd));
            }
            catch (COMException)
            {
            }
            catch (InvalidCastException)
            {
            }
        }

        private static object? FindRunningAutoCad()
        {
            object? app = TryGetActive("AutoCAD.Application");
            if (app != null)
                return app;

            for (int version = 30; version >= 15; version--)
            {
                app = TryGetActive("AutoCAD.Application." + version.ToString(CultureInfo.InvariantCulture));
                if (app != null)
                    return app;
            }

            return FindInRunningObjectTable();
        }

        private static object? TryGetActive(string progId)
        {
            if (CLSIDFromProgID(progId, out Guid clsid) != 0)
                return null;

            if (GetActiveObject(ref clsid, IntPtr.Zero, out object instance) != 0)
                return null;

            return instance;
        }

        private static object? FindInRunningObjectTable()
        {
            if (GetRunningObjectTable(0, out IRunningObjectTable rot) != 0)
                return null;

            rot.EnumRunning(out IEnumMoniker enumerator);
            var monikers = new IMoniker[1];
            object? visible = null;
            object? any = null;
            IntPtr fetched = Marshal.AllocHGlobal(sizeof(int));

            try
            {
                while (true)
                {
                    Marshal.WriteInt32(fetched, 0);
                    if (enumerator.Next(1, monikers, fetched) != 0 || Marshal.ReadInt32(fetched) == 0)
                        break;

                    IMoniker moniker = monikers[0];
                    CreateBindCtx(0, out IBindCtx ctx);
                    moniker.GetDisplayName(ctx, null, out string displayName);
                    if (displayName.Contains("AutoCAD.Application", StringComparison.OrdinalIgnoreCase))
                    {
                        rot.GetObject(moniker, out object candidate);
                        if (visible == null && IsVisible(candidate))
                            visible = candidate;
                        any ??= candidate;
                    }

                    Marshal.ReleaseComObject(ctx);
                    Marshal.ReleaseComObject(moniker);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(fetched);
                Marshal.ReleaseComObject(enumerator);
                Marshal.ReleaseComObject(rot);
            }

            return visible ?? any;
        }

        private static bool IsVisible(object candidate)
        {
            try
            {
                return (bool)((dynamic)candidate).Visible;
            }
            catch (COMException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int CLSIDFromProgID(string progId, out Guid clsid);

        [DllImport("oleaut32.dll", PreserveSig = true)]
        private static extern int GetActiveObject(
            ref Guid clsid,
            IntPtr reserved,
            [MarshalAs(UnmanagedType.IUnknown)] out object instance);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, [MarshalAs(UnmanagedType.Interface)] out IRunningObjectTable rot);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ctx);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static string? _draftDrawingPath;
        private static string? _draftHandle;
        private static readonly Dictionary<string, List<string>> _startMarkerHandles =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<string>> _endMarkerHandles =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<string>> _stationMarkerHandles =
            new(StringComparer.OrdinalIgnoreCase);

        public static string DrawDraftRoad(
            IReadOnlyList<Las.RoadVertex> vertices)
        {
            if (vertices.Count < 2)
                return "CAD에 표시할 노선 점이 부족합니다.";

            object? appObject = FindRunningAutoCad();

            if (appObject == null)
                return "실행 중인 AutoCAD에 연결하지 못했습니다.";

            try
            {
                dynamic app = appObject;
                dynamic doc = app.ActiveDocument;
                doc.Activate();

                string drawingPath =
                    Convert.ToString(doc.FullName) ?? "";

                const string layerName = "ROAD_DRAFT_UNVERIFIED";
                EnsureLayer(doc, layerName);

                double[] coordinates = new double[vertices.Count * 3];

                for (int i = 0; i < vertices.Count; i++)
                {
                    coordinates[i * 3] = vertices[i].X;
                    coordinates[i * 3 + 1] = vertices[i].Y;
                    coordinates[i * 3 + 2] = vertices[i].Z;
                }

                dynamic polyline =
                    doc.ModelSpace.Add3DPoly(coordinates);

                polyline.Layer = layerName;
                polyline.Color = 1; // 빨간색

                // 이 프로그램이 직전에 표시한 초안만 지운다.
                if (_draftDrawingPath == drawingPath &&
                    !string.IsNullOrEmpty(_draftHandle))
                {
                    try
                    {
                        dynamic previous =
                            doc.HandleToObject(_draftHandle);
                        previous.Delete();
                    }
                    catch (COMException)
                    {
                        // 이전 초안이 CAD에서 이미 삭제된 경우
                    }
                }

                _draftDrawingPath = drawingPath;
                _draftHandle = Convert.ToString(polyline.Handle);

                int stationCount = MarkBendStations(doc, layerName, vertices);

                doc.Regen(1);

                string drawingName = Convert.ToString(doc.Name) ?? "도면";
                return $"CAD 도면 {drawingName}에 초안 노선을 표시했습니다. 20m 간격 점 {stationCount}곳에 P1부터 표시했습니다.";
            }
            catch (COMException ex)
            {
                return "CAD 초안 표시 실패: " + ex.Message;
            }
        }

        private static int MarkBendStations(
            dynamic doc,
            string layerName,
            IReadOnlyList<Las.RoadVertex> vertices)
        {
            EnsureMarkTextStyle(doc);

            var created = new List<string>();
            const double pointRadius = 0.7;
            const double textHeight = 2.4;
            const double textGap = 1.6;

            for (int index = 0; index < vertices.Count; index++)
            {
                Las.RoadVertex point = vertices[index];
                dynamic circle = doc.ModelSpace.AddCircle(
                    new[] { point.X, point.Y, point.Z },
                    pointRadius);
                circle.Layer = layerName;
                circle.Color = 7;
                SetRgb(circle, 255, 255, 255);
                created.Add(Convert.ToString(circle.Handle) ?? "");

                (double offsetX, double offsetY) = BendLabelOffset(vertices, index, pointRadius + textGap);
                dynamic text = AddLabelText(
                    doc,
                    layerName,
                    "P" + (index + 1).ToString(CultureInfo.InvariantCulture),
                    point.X + offsetX,
                    point.Y + offsetY,
                    point.Z,
                    textHeight);
                text.Color = 7;
                SetRgb(text, 255, 255, 255);
                created.Add(Convert.ToString(text.Handle) ?? "");
            }

            ReplaceMarkers(doc, _stationMarkerHandles, DrawingKey(doc), created);
            return vertices.Count;
        }

        private static (double X, double Y) BendLabelOffset(
            IReadOnlyList<Las.RoadVertex> vertices,
            int index,
            double gap)
        {
            int next = index < vertices.Count - 1 ? index + 1 : index;
            int previous = next == index ? index - 1 : index;
            double dx = vertices[next].X - vertices[previous].X;
            double dy = vertices[next].Y - vertices[previous].Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.0001)
                return (gap, gap);

            return (-dy / length * gap, dx / length * gap);
        }

        public static string MarkPickedPoint(bool isStart, double x, double y, double z)
        {
            object? appObject = FindRunningAutoCad();
            if (appObject == null)
                return "좌표는 넣었지만, 실행 중인 AutoCAD에 기호를 그리지 못했습니다.";

            try
            {
                dynamic app = appObject;
                dynamic doc = app.ActiveDocument;
                doc.Activate();

                string drawingPath = DrawingKey(doc);
                const string layerName = "ROAD_PICK";
                EnsureLayer(doc, layerName);
                EnsureMarkTextStyle(doc);

                object document = doc;
                (double radius, double textHeight) = MarkerSize(document);
                string label = isStart ? "시점" : "종점";
                var created = new List<string>();

                try
                {
                    doc.SetVariable("FILLMODE", 1);
                }
                catch (COMException)
                {
                }

                AddFilledCircle(doc, layerName, 1, 220, 30, 30, x, y, z, radius, created);

                AddMarkerLabel(doc, layerName, label, x, y, z, radius, textHeight, created);

                Dictionary<string, List<string>> store = isStart
                    ? _startMarkerHandles
                    : _endMarkerHandles;
                ReplaceMarkers(doc, store, drawingPath, created);

                doc.Regen(1);

                string shape = "빨간색으로 채운 원";
                string drawingName = Convert.ToString(doc.Name) ?? "도면";
                return $"{drawingName}에 {label} 기호({shape})를 표시했습니다.";
            }
            catch (COMException ex)
            {
                return "좌표는 넣었지만 도면 기호 표시에 실패했습니다. " + ex.Message;
            }
        }

        private static string DrawingKey(dynamic doc)
        {
            string path = Convert.ToString(doc.FullName) ?? "";
            if (!string.IsNullOrWhiteSpace(path))
                return path;

            return Convert.ToString(doc.Name) ?? "";
        }

        private static void EnsureLayer(dynamic doc, string layerName)
        {
            try
            {
                _ = doc.Layers.Item(layerName);
            }
            catch (COMException)
            {
                _ = doc.Layers.Add(layerName);
            }
        }

        private static void EnsureMarkTextStyle(dynamic doc)
        {
            const string styleName = "ROAD_PICK";
            dynamic style;
            try
            {
                style = doc.TextStyles.Item(styleName);
            }
            catch (COMException)
            {
                style = doc.TextStyles.Add(styleName);
            }

            try
            {
                style.SetFont("맑은 고딕", true, false, 129, 34);
            }
            catch (COMException)
            {
                try
                {
                    style.SetFont("Malgun Gothic", true, false, 129, 34);
                }
                catch (COMException)
                {
                }
            }
        }

        private static (double Radius, double TextHeight) MarkerSize(dynamic doc)
        {
            double viewSize = 400;
            try
            {
                double value = Convert.ToDouble(
                    doc.GetVariable("VIEWSIZE"),
                    CultureInfo.InvariantCulture);
                if (value > 0)
                    viewSize = value;
            }
            catch (COMException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (FormatException)
            {
            }

            double radius = Math.Max(viewSize * 0.008, 0.4);
            double textHeight = Math.Max(viewSize * 0.02, 1.6);
            return (radius, textHeight);
        }

        private static void AddFilledCircle(
            dynamic doc,
            string layerName,
            int color,
            int red,
            int green,
            int blue,
            double x,
            double y,
            double z,
            double radius,
            List<string> created)
        {
            // 중심선 반지름의 두 배 폭이라 안쪽이 비지 않은 색 원이다.
            double centerline = radius / 2.0;
            dynamic disk = doc.ModelSpace.AddLightWeightPolyline(new[]
            {
                x + centerline, y,
                x - centerline, y
            });
            disk.Closed = true;
            disk.SetBulge(0, 1.0);
            disk.SetBulge(1, 1.0);
            disk.SetWidth(0, radius, radius);
            disk.SetWidth(1, radius, radius);
            disk.Elevation = z;
            disk.Layer = layerName;
            disk.Color = color;
            SetRgb(disk, red, green, blue);
            created.Add(Convert.ToString(disk.Handle) ?? "");
        }

        private static void AddMarkerLabel(
            dynamic doc,
            string layerName,
            string label,
            double x,
            double y,
            double z,
            double radius,
            double textHeight,
            List<string> created)
        {
            double left = x + radius * 1.7;
            double baseline = y + radius * 0.2;
            double shadowOffset = textHeight * 0.08;

            dynamic shadow = AddLabelText(
                doc,
                layerName,
                label,
                left + shadowOffset,
                baseline - shadowOffset,
                z,
                textHeight);
            shadow.Color = 7;
            SetRgb(shadow, 20, 20, 20);
            created.Add(Convert.ToString(shadow.Handle) ?? "");

            dynamic text = AddLabelText(doc, layerName, label, left, baseline, z, textHeight);
            text.Color = 7;
            SetRgb(text, 255, 255, 255);
            created.Add(Convert.ToString(text.Handle) ?? "");
            BringToFront(doc, text);
        }

        private static dynamic AddLabelText(
            dynamic doc,
            string layerName,
            string label,
            double x,
            double y,
            double z,
            double textHeight)
        {
            dynamic text = doc.ModelSpace.AddText(label, new[] { x, y, z }, textHeight);
            text.Layer = layerName;
            try
            {
                text.StyleName = "ROAD_PICK";
            }
            catch (COMException)
            {
            }

            text.Height = textHeight;
            return text;
        }

        private static void SetRgb(dynamic entity, int red, int green, int blue)
        {
            try
            {
                dynamic trueColor = entity.TrueColor;
                trueColor.SetRGB(red, green, blue);
                entity.TrueColor = trueColor;
            }
            catch (COMException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
            }
        }

        private static void BringToFront(dynamic doc, dynamic entity)
        {
            try
            {
                dynamic sort = doc.ModelSpace.GetExtensionDictionary().GetObject("ACAD_SORTENTS");
                sort.MoveToTop(new object[] { entity });
            }
            catch (COMException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
            }
        }

        private static void ReplaceMarkers(
            dynamic doc,
            Dictionary<string, List<string>> store,
            string drawingPath,
            List<string> created)
        {
            if (!store.TryGetValue(drawingPath, out List<string>? previous))
            {
                previous = new List<string>();
                store[drawingPath] = previous;
            }

            foreach (string handle in previous)
            {
                if (string.IsNullOrEmpty(handle))
                    continue;

                try
                {
                    dynamic entity = doc.HandleToObject(handle);
                    entity.Delete();
                }
                catch (COMException)
                {
                }
            }

            previous.Clear();
            previous.AddRange(created.Where(handle => !string.IsNullOrEmpty(handle)));
        }
    }
}
