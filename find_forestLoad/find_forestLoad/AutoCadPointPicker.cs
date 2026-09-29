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

                // 초안만 넣을 전용 레이어를 준비한다.
                try
                {
                    _ = doc.Layers.Item(layerName);
                }
                catch (COMException)
                {
                    _ = doc.Layers.Add(layerName);
                }

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

                doc.Regen(1);

                return $"CAD 도면 {doc.Name}에 초안 노선을 표시했습니다.";
            }
            catch (COMException ex)
            {
                return "CAD 초안 표시 실패: " + ex.Message;
            }
        }
    }
}
