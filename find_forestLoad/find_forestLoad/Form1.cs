using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using find_forestLoad.Las;

namespace find_forestLoad
{
    public partial class Form1 : Form
    {
        private const double GridCellSizeMeters = 1;
        private const double MaxRadiusMeters = 200;

        private ElevationGrid? _grid;
        private string? _selectedPath;
        private string? _loadedPath;
        private long _loadedLength;
        private DateTime _loadedWriteTimeUtc;
        private CancellationTokenSource? _cts;
        private bool _busy;
        private ForestRoadPath? _road;
        private readonly Dictionary<RoadObjective, IReadOnlyList<RoadDesignVertex>>
            _candidateProfiles = new();

        public Form1()
        {
            InitializeComponent();
            AcceptButton = buttonQueryZ;
            textBox_radius.Text = "2";
            textBox_slope.PlaceholderText = "비우면 선택 임도의 최댓값";
            label5.Text = "허용 경사도(%)";
            buttonOpenLas.Click += buttonOpenLas_Click;
            buttonQueryZ.Click += buttonQueryZ_Click;
            buttonPickStart.Click += (_, _) => PickFromAutoCad(textBox_x1, textBox_y1, "시점을 클릭하세요");
            buttonPickEnd.Click += (_, _) => PickFromAutoCad(textBox_x2, textBox_y2, "종점을 클릭하세요");
            button1.Click += buttonRoad_Click;
            buttonPreviewCad.Click += buttonPreviewCad_Click;
            FormClosing += Form1_FormClosing;
            textBox_result.Text =
                "LAS 파일을 연 다음, 시점과 종점의 X/Y를 입력하고 고도 조회를 누르세요." + Environment.NewLine +
                "AutoCAD에 DWG를 열어 두면 시점을 CAD에서, 종점을 CAD에서 버튼으로 점을 찍을 수 있습니다." + Environment.NewLine +
                "허용 경사도(%)를 입력하면 그 한도로 경로를 찾습니다. 비워 두면 임도 기준표 값을 쓰고, 임도 생성 창에서도 바꿀 수 있습니다." + Environment.NewLine +
                "첫 조회에서 파일 전체를 한 번 읽습니다. 2GB를 넘으면 몇 분 걸릴 수 있습니다.";
        }

        private void buttonPreviewCad_Click(object? sender, EventArgs e)
        {
            if (_busy)
                return;

            if (_candidateProfiles.Count == 0)
            {
                MessageBox.Show(this,
                    "표시할 수 있는 후보가 없습니다. 경로가 같은 위치를 되돌아 지나가거나 종단 검증에 실패했습니다. ",
                    "후보 노선",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var dialog = new Form
            {
                Text = "CAD에 표시할 후보 노선 선택",
                Width = 420,
                Height = 300,
                StartPosition = FormStartPosition.CenterParent
            };

            var list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(12)
            };

            dialog.Controls.Add(list);

            RoadObjective? selected = null;

            foreach (RoadObjective objective in Enum.GetValues<RoadObjective>())
            {
                if (!_candidateProfiles.TryGetValue(objective, out var profile))
                    continue;

                string name = objective == RoadObjective.Distance
                    ? "거리 후보 / 부드러운 우회 초안"
                    : ObjectiveCaption(objective);

                var candidateButton = new Button
                {
                    Text = $"{name}  ·  꼭짓점 {profile.Count}개",
                    Width = 365,
                    Height = 48,
                    Margin = new Padding(0, 0, 0, 8),
                    Tag = objective
                };

                candidateButton.Click += (_, _) =>
                {
                    selected = (RoadObjective)candidateButton.Tag!;
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };

                list.Controls.Add(candidateButton);
            }

            if (dialog.ShowDialog(this) != DialogResult.OK || selected == null)
                return;

            IReadOnlyList<RoadDesignVertex> chosen = _candidateProfiles[selected.Value];

            RoadVertex[] vertices = chosen
                .Select(point => new RoadVertex(
                    point.X, point.Y, point.RoadZ))
                .ToArray();

            string message = AutoCadPointPicker.DrawDraftRoad(vertices);

            MessageBox.Show(this,
                message,
                "CAD 미리보기",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static string ObjectiveCaption(RoadObjective objective) =>
            objective switch
            {
                RoadObjective.Safety => "안정성",
                RoadObjective.Cost => "비용 추정",
                RoadObjective.Distance => "거리 후보",
                RoadObjective.ConstructionTime => "공사 기간 추정",
                _ => objective.ToString()
            };

        private void PickFromAutoCad(TextBox xBox, TextBox yBox, string prompt)
        {
            if (_busy)
                return;

            textBox_result.Text = "AutoCAD 창으로 이동합니다. 도면에서 점을 클릭하세요. Esc로 취소합니다.";
            Refresh();

            AutoCadPickResult result = AutoCadPointPicker.TryPick(prompt);
            BringThisWindowAboveAutoCad();

            if (result.Cancelled)
            {
                textBox_result.Text = "점 선택이 취소되었습니다.";
                return;
            }

            if (!result.Ok)
            {
                textBox_result.Text = result.Message ?? "점을 가져오지 못했습니다.";
                MessageBox.Show(this, textBox_result.Text, "AutoCAD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            xBox.Text = result.X.ToString("0.000", CultureInfo.CurrentCulture);
            yBox.Text = result.Y.ToString("0.000", CultureInfo.CurrentCulture);
            textBox_result.Text =
                result.DrawingName + " 에서 클릭한 좌표를 넣었습니다." + Environment.NewLine +
                "X " + xBox.Text + "    Y " + yBox.Text + "    도면 Z " + result.Z.ToString("0.000", CultureInfo.CurrentCulture) + Environment.NewLine +
                "도면 좌표계가 LAS와 같아야 고도가 맞습니다. 고도는 고도 조회로 LAS에서 가져옵니다.";
            if (!string.IsNullOrWhiteSpace(result.Message))
                textBox_result.AppendText(Environment.NewLine + result.Message);
        }

        private void BringThisWindowAboveAutoCad()
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;

            IntPtr handle = Handle;
            IntPtr foreground = GetForegroundWindow();
            uint foregroundThread = GetWindowThreadProcessId(foreground, IntPtr.Zero);
            uint thisThread = GetCurrentThreadId();
            bool attached = false;

            try
            {
                if (foreground != handle && foregroundThread != 0 && foregroundThread != thisThread)
                    attached = AttachThreadInput(foregroundThread, thisThread, true);

                const uint flags = SwpNoMove | SwpNoSize | SwpShowWindow;
                ShowWindow(handle, SwRestore);
                SetWindowPos(handle, HwndTopMost, 0, 0, 0, 0, flags);
                SetWindowPos(handle, HwndNoTopMost, 0, 0, 0, 0, flags);
                BringWindowToTop(handle);
                SetForegroundWindow(handle);
            }
            finally
            {
                if (attached)
                    AttachThreadInput(foregroundThread, thisThread, false);
            }

            Activate();
        }

        private const uint SwpNoMove = 0x0002;
        private const uint SwpNoSize = 0x0001;
        private const uint SwpShowWindow = 0x0040;
        private const int SwRestore = 9;
        private static readonly IntPtr HwndTopMost = new(-1);
        private static readonly IntPtr HwndNoTopMost = new(-2);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool attach);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int command);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr insertAfter,
            int x,
            int y,
            int width,
            int height,
            uint flags);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void buttonOpenLas_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "LAS 파일 (*.las)|*.las",
                Title = "LAS 파일 선택",
                CheckFileExists = true
            };

            if (!string.IsNullOrEmpty(_selectedPath))
                dialog.InitialDirectory = Path.GetDirectoryName(_selectedPath);

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            string fullPath = Path.GetFullPath(dialog.FileName);
            if (!IsCurrentGrid(fullPath))
            {
                _grid = null;
                _loadedPath = null;
            }

            _selectedPath = fullPath;
            labelLasFile.Text = Path.GetFileName(fullPath);
            textBox_z1.Clear();
            textBox_z2.Clear();
            progressBar1.Value = 0;
            textBox_result.Text =
                fullPath + Environment.NewLine +
                "고도 조회를 누르면 파일 전체를 한 번 읽어 고도 격자를 만듭니다." + Environment.NewLine +
                "2GB를 넘으면 몇 분 걸릴 수 있습니다.";
        }

        private async void buttonQueryZ_Click(object? sender, EventArgs e)
        {
            if (_busy)
                return;

            if (!TryReadInputs(out QueryInput input))
                return;

            if (string.IsNullOrEmpty(_selectedPath))
            {
                MessageBox.Show(this, "LAS 파일을 선택하세요.", "고도 조회", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _busy = true;
            SetBusy(true);
            textBox_z1.Clear();
            textBox_z2.Clear();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                bool reused = IsCurrentGrid(_selectedPath);
                ElevationGrid? grid = await LoadGridAsync();
                if (IsDisposed || grid == null)
                    return;

                ShowElevations(grid, input, reused);
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed)
                    textBox_result.Text = "조회가 취소되었습니다.";
            }
            catch (Exception ex)
            {
                if (IsDisposed)
                    return;

                progressBar1.Value = 0;
                textBox_result.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "고도 조회 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _busy = false;
                _cts?.Dispose();
                _cts = null;
                if (!IsDisposed)
                    SetBusy(false);
            }
        }

        private async void buttonRoad_Click(object? sender, EventArgs e)
        {
            if (_busy)
                return;

            if (string.IsNullOrEmpty(_selectedPath))
            {
                MessageBox.Show(this, "LAS 파일을 선택하세요.", "임도 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryReadRoadInput(out double x1,out double y1,out double x2,out double y2,out double radius))
                return;

            if (!TryReadOptionalSlope(out double? typedSlope))
                return;

            using var standardDialog = new RoadStandardDialog(typedSlope);

            if (standardDialog.ShowDialog(this) != DialogResult.OK)
                return;

            RoadStandard standard = standardDialog.SelectedStandard;
            double slopePercent = standard.MaximumGradePercent;

            textBox_slope.Text = slopePercent.ToString("0.#", CultureInfo.CurrentCulture);

            _busy = true;
            SetBusy(true);
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                _candidateProfiles.Clear();
                _road = null;

                ElevationGrid? grid = await LoadGridAsync();
                if (IsDisposed || grid == null)
                    return;

                textBox_result.Text = "부드러운 우회 경로를 먼저 검사하는 중입니다.";

                DesignedRoadSearchResult smoothResult = await Task.Run(
                    () => grid.FindSmoothDetourRoad(
                        x1, y1, x2, y2,
                        radius,
                        slopePercent,
                        RoadStandard.PreliminaryCutFillLimitMeters,
                        standard.MinimumCenterlineCurveRadiusMeters,
                        _cts.Token),
                    _cts.Token);

                if (IsDisposed)
                    return;

                if (smoothResult.Succeeded &&
                    smoothResult.Vertices is { Count: > 1 } smoothPath)
                {
                    _candidateProfiles[RoadObjective.Distance] = smoothPath;

                    double length = 0;

                    for (int i = 1; i < smoothPath.Count; i++)
                    {
                        double dx = smoothPath[i].X - smoothPath[i - 1].X;
                        double dy = smoothPath[i].Y - smoothPath[i - 1].Y;
                        length += Math.Sqrt(dx * dx + dy * dy);
                    }

                    progressBar1.Value = progressBar1.Maximum;

                    textBox_result.Text =
                        $"부드러운 우회 초안 1개를 찾았습니다. 길이 {length:0.0}m" +
                        Environment.NewLine +
                        "후보 노선 선택에서 해당 선을 CAD에 표시할 수 있습니다." +
                        Environment.NewLine +
                        "경사·중심선 절토·성토·곡률 반지름을 예비 검사했습니다." +
                        Environment.NewLine +
                        "도로 폭·배수·사면과 실제 시공 설계는 검증 전입니다.";

                    return;
                }

                textBox_result.Text = "도로 경사와 절토, 성토 한도를 고려해 후보 경로를 함께 찾는 중입니다.";
                var progress = new Progress<ElevationBuildProgress>(OnBuildProgress);
                RoadObjective[] objectives = Enum.GetValues<RoadObjective>();
                var searchTasks = objectives.Select(objective => Task.Run(
                    () => (
                        objective,
                        result: grid.FindDesignedRoad(
                            x1, y1, x2, y2,
                            radius,
                            slopePercent,
                            RoadStandard.PreliminaryCutFillLimitMeters,
                            _cts.Token,
                            objective,
                            progress)),
                    _cts.Token)).ToArray();

                (RoadObjective objective, DesignedRoadSearchResult result)[] finished =
                    await Task.WhenAll(searchTasks);

                if (IsDisposed)
                    return;

                var searchResults = new Dictionary<RoadObjective, DesignedRoadSearchResult>();
                foreach ((RoadObjective objective, DesignedRoadSearchResult candidateResult) in finished)
                {
                    searchResults[objective] = candidateResult;

                    if (candidateResult.Succeeded &&
                        candidateResult.Vertices is { Count: > 1 } designedPath &&
                        RoadProfile.TryAcceptDesigned(
                            designedPath,
                            slopePercent,
                            RoadStandard.PreliminaryCutFillLimitMeters,
                            out _) &&
                        !RoadGeometry.HasRepeatedPlanPoint(designedPath))
                    {
                        _candidateProfiles[objective] = designedPath;
                    }
                }

                DesignedRoadSearchResult result = searchResults[RoadObjective.Cost];

                if (IsDisposed)
                    return;

                if (!result.Succeeded || result.Vertices is not { Count: > 1 })
                {
                    _road = null;
                    progressBar1.Value = 0;
                    string other = _candidateProfiles.Count > 0
                        ? Environment.NewLine + "다른 목표의 후보는 아래 버튼에서 CAD로 볼 수 있습니다."
                        : "";
                    textBox_result.Text =
                        (smoothResult.Failure ?? "부드러운 우회 후보 없음") +
                        Environment.NewLine +
                        Environment.NewLine +
                        (result.Failure ?? "경로를 찾지 못했습니다.") +
                        other;
                    return;
                }

                IReadOnlyList<RoadDesignVertex> designed = result.Vertices;

                if (!RoadProfile.TryAcceptDesigned(
                        designed,
                        slopePercent,
                        RoadStandard.PreliminaryCutFillLimitMeters,
                        out string profileReason))
                {
                    _road = null;
                    progressBar1.Value = 0;
                    textBox_result.Text = profileReason;
                    return;
                }

                bool straightAccepted = grid.TryBuildStraightRoad(
                    designed,
                    slopePercent,
                    RoadStandard.PreliminaryCutFillLimitMeters,
                    out IReadOnlyList<RoadDesignVertex> straightCandidate,
                    out string straightReason);

                if (straightAccepted)
                {
                    designed = straightCandidate;
                    _candidateProfiles[RoadObjective.Distance] = straightCandidate;
                }

                string candidateSummary = string.Join(
                    Environment.NewLine,
                    Enum.GetValues<RoadObjective>().Select(objective =>
                        _candidateProfiles.TryGetValue(objective, out var profile)
                            ? $"{objective}: 종단 검증 통과, 꼭짓점 {profile.Count}개"
                            : $"{objective}: 사용 가능한 후보 없음"));

                if (RoadGeometry.HasRepeatedPlanPoint(designed))
                {
                    _road = null;
                    progressBar1.Value = 0;

                    textBox_result.Text =
                        candidateSummary +
                        Environment.NewLine +
                        Environment.NewLine +
                        "현재 경로는 같은 평면 위치를 다시 지나므로 임도 초안으로 사용할 수 없습니다." +
                        Environment.NewLine +
                        "경사를 맞추기 위해 되돌아간 구간이 있습니다. 다른 경로를 탐색해야 합니다.";

                    return;
                }

                RoadVertex[] roadVertices = designed
                    .Select(point => new RoadVertex(
                        point.X, point.Y, point.RoadZ))
                    .ToArray();

                var segments = new List<RoadSegment>();
                double maximumStepGrade = 0;

                for (int i = 1; i < roadVertices.Length; i++)
                {
                    RoadVertex from = roadVertices[i - 1];
                    RoadVertex to = roadVertices[i];

                    double dx = to.X - from.X;
                    double dy = to.Y - from.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    double grade = distance > 0
                        ? (to.Z - from.Z) / distance * 100
                        : 0;

                    segments.Add(new RoadSegment(
                        from, to, distance, grade));

                    maximumStepGrade =
                        Math.Max(maximumStepGrade, Math.Abs(grade));
                }

                _road = new ForestRoadPath(
                    roadVertices,
                    segments,
                    maximumStepGrade,
                    0);

                progressBar1.Value = progressBar1.Maximum;

                textBox_z1.Text =
                    roadVertices[0].Z.ToString("0.000", CultureInfo.CurrentCulture);
                textBox_z2.Text =
                    roadVertices[^1].Z.ToString("0.000", CultureInfo.CurrentCulture);

                double maximumCut = designed.Max(point => point.CutHeight);
                double maximumFill = designed.Max(point => point.FillHeight);

                IReadOnlyList<RoadCorner> sharpCorners =
                    RoadGeometry.FindSharpCorners(designed);

                double trialRadius = standard.MinimumCenterlineCurveRadiusMeters ?? 15.0;
                string radiusNote = standard.MinimumCenterlineCurveRadiusMeters.HasValue
                    ? "기준 반지름을 사용한 곡선 공간 예비 진단"
                    : "15m를 가정한 곡선 공간 예비 진단 (작업임도 법정값 아님)";

                IReadOnlyList<CurveSpaceIssue> curveIssues =
                    RoadGeometry.FindCurveSpaceIssues(designed, trialRadius);

                textBox_result.Text =
                    candidateSummary +
                    Environment.NewLine +
                    Environment.NewLine +
                    straightReason +
                    Environment.NewLine +
                    $"초안 최대 절토 높이: {maximumCut:0.00}m" +
                    Environment.NewLine +
                    $"초안 최대 성토 높이: {maximumFill:0.00}m" +
                    Environment.NewLine +
                    $"25° 초과 방향 변경: {sharpCorners.Count}곳 (곡선 설계 전)" +
                    Environment.NewLine +
                    $"반지름 {trialRadius:0.#}m 곡선 공간 부족: " +
                    $"{curveIssues.Count}구간 ({radiusNote})" +
                    Environment.NewLine +
                    "도로 폭·곡선·배수·사면은 아직 검증 전입니다." +
                    Environment.NewLine +
                    Environment.NewLine +
                    DescribeRoad(_road, standard);
            }

            catch (OperationCanceledException)
            {
                if (!IsDisposed)
                    textBox_result.Text = "임도 경로 찾기가 취소되었습니다.";
            }
            catch (Exception ex)
            {
                if (IsDisposed)
                    return;

                progressBar1.Value = 0;
                textBox_result.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "임도 생성 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _busy = false;
                _cts?.Dispose();
                _cts = null;
                if (!IsDisposed)
                    SetBusy(false);
            }
        }

        private async Task<ElevationGrid?> LoadGridAsync()
        {
            if (string.IsNullOrEmpty(_selectedPath))
                return null;

            if (IsCurrentGrid(_selectedPath))
                return _grid;

            progressBar1.Value = 0;
            textBox_result.Text = "LAS 파일을 읽는 중입니다. 2GB를 넘으면 몇 분 걸릴 수 있습니다.";
            var progress = new Progress<ElevationBuildProgress>(OnBuildProgress);
            ElevationGrid built = await ElevationGrid.BuildAsync(
                _selectedPath,
                GridCellSizeMeters,
                progress,
                _cts?.Token ?? CancellationToken.None);

            if (IsDisposed)
                return null;

            var info = new FileInfo(_selectedPath);
            _grid = built;
            _loadedPath = info.FullName;
            _loadedLength = info.Length;
            _loadedWriteTimeUtc = info.LastWriteTimeUtc;
            progressBar1.Value = progressBar1.Maximum;
            return _grid;
        }

        private bool TryReadRoadInput(
        out double x1,
        out double y1,
        out double x2,
        out double y2,
        out double radius)
        {
            x1 = y1 = x2 = y2 = radius = 0;
            if (!TryParseNumber(textBox_x1.Text, out x1) || !TryParseNumber(textBox_y1.Text, out y1))
            {
                MessageBox.Show(this, "시점 X/Y를 숫자로 입력하세요.", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!TryParseNumber(textBox_x2.Text, out x2) || !TryParseNumber(textBox_y2.Text, out y2))
            {
                MessageBox.Show(this, "종점 X/Y를 숫자로 입력하세요.", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!TryParseNumber(textBox_radius.Text, out radius) || radius <= 0 || radius > MaxRadiusMeters)
            {
                MessageBox.Show(this, "검색 반경(m)은 0보다 크고 200 이하인 숫자로 입력하세요.", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        private bool TryReadOptionalSlope(out double? slopePercent)
        {
            slopePercent = null;
            string text = textBox_slope.Text.Trim().TrimEnd('%').Trim();
            if (text.Length == 0)
                return true;

            if (!TryParseNumber(text, out double value) || value <= 0 || value > 100)
            {
                MessageBox.Show(
                    this,
                    "허용 경사도(%)는 비워 두거나, 0보다 크고 100 이하인 숫자로 입력하세요.",
                    "입력",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return false;
            }

            slopePercent = value;
            return true;
        }

        private static string DescribeRoad(ForestRoadPath road, RoadStandard standard)
        {
            var report = new StringBuilder();report.Append(standard.KindName).Append(" · ").Append(standard.Terrain == TerrainKind.Normal ? "일반지형" : "특수지형").Append(" · 설계속도 ").Append(standard.DesignSpeedKmh).AppendLine("km/h");

            report.Append("적용 경사 한도 ")
                .Append(standard.MaximumGradePercent.ToString("0.##", CultureInfo.CurrentCulture))
                .AppendLine("%");
            report.Append("수평 길이 ").Append(road.LengthMeters.ToString("0.0", CultureInfo.CurrentCulture)).Append(" m");
            report.Append("  |  구간 최대 경사 ").Append(road.MaxSlopePercent.ToString("0.0", CultureInfo.CurrentCulture)).Append('%');
            report.Append("  |  칸 사이 최대 경사 ").Append(road.MaxStepSlopePercent.ToString("0.0", CultureInfo.CurrentCulture)).AppendLine("%");
            int middle = Math.Max(0, road.Vertices.Count - 2);
            report.Append("꼭짓점 ").Append(road.Vertices.Count).Append("개 (시점, 중간점 ").Append(middle).AppendLine("개, 종점)");
            report.AppendLine("후보 노선 선택 후 CAD에서 보기로 도면에 표시할 수 있습니다.");

            int count = road.Segments.Count;
            if (count == 0)
            {
                RoadVertex only = road.Vertices[0];
                report.Append("시점과 종점이 같은 칸입니다. Z ").AppendLine(only.Z.ToString("0.000", CultureInfo.CurrentCulture));
                return report.ToString();
            }

            bool summarize = count > 24;
            for (int i = 0; i < count; i++)
            {
                if (summarize && i == 8)
                {
                    report.Append("... 중간 ").Append(count - 13).AppendLine("개 구간 생략 ...");
                    i = count - 5;
                }

                AppendSegment(report, road.Segments[i], i, count);
            }

            return report.ToString();
        }

        private static void AppendSegment(StringBuilder report, RoadSegment segment, int index, int count)
        {
            string fromName = index == 0 ? "시점" : "중간점";
            string toName = index == count - 1 ? "종점" : "중간점";
            report.Append(index + 1).Append(". ").Append(fromName).Append(" → ").Append(toName);
            report.Append("  ").Append(segment.HorizontalMeters.ToString("0.0", CultureInfo.CurrentCulture)).Append(" m");
            report.Append("  경사 ").Append(segment.SlopePercent.ToString("+0.0;-0.0;0.0", CultureInfo.CurrentCulture)).AppendLine("%");
            report.Append("    (").Append(FormatCoord(segment.From.X)).Append(", ").Append(FormatCoord(segment.From.Y));
            report.Append(", Z ").Append(segment.From.Z.ToString("0.000", CultureInfo.CurrentCulture)).Append(')');
            report.Append(" → (").Append(FormatCoord(segment.To.X)).Append(", ").Append(FormatCoord(segment.To.Y));
            report.Append(", Z ").Append(segment.To.Z.ToString("0.000", CultureInfo.CurrentCulture)).AppendLine(")");
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
        }

        private void OnBuildProgress(ElevationBuildProgress progress)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            try
            {
                int max = progressBar1.Maximum;
                progressBar1.Value = (int)Math.Clamp(Math.Round(progress.Fraction * max), 0, max);
                textBox_result.Text = progress.Message;
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void ShowElevations(ElevationGrid grid, QueryInput input, bool reused)
        {
            if (input.HasStart)
            {
                ElevationSample? start = grid.GetElevation(input.X1, input.Y1, input.Radius);
                textBox_z1.Text = start is null ? "없음" : start.Value.Z.ToString("0.000", CultureInfo.CurrentCulture);
            }

            if (input.HasEnd)
            {
                ElevationSample? end = grid.GetElevation(input.X2, input.Y2, input.Radius);
                textBox_z2.Text = end is null ? "없음" : end.Value.Z.ToString("0.000", CultureInfo.CurrentCulture);
            }

            textBox_result.Text = BuildReport(grid, input, reused);
        }

        private static string BuildReport(ElevationGrid grid, QueryInput input, bool reused)
        {
            LasHeader header = grid.Header;
            var info = new FileInfo(grid.SourcePath);
            var report = new StringBuilder();

            if (reused)
                report.AppendLine("이전에 읽어 둔 고도 격자를 사용했습니다.");

            report.AppendLine(grid.SourcePath);
            report.Append("크기 ").Append(FormatSize(info.Exists ? info.Length : 0));
            report.Append("  |  LAS ").Append(header.VersionMajor).Append('.').Append(header.VersionMinor);
            report.Append("  |  점 형식 ").Append(header.PointDataFormat);
            report.Append("  |  ").Append(header.PointCount.ToString("N0", CultureInfo.CurrentCulture)).AppendLine("점");

            if (!string.IsNullOrWhiteSpace(header.GeneratingSoftware))
                report.Append("생성 프로그램: ").AppendLine(header.GeneratingSoftware);

            report.Append("X ").Append(FormatCoord(header.MinX)).Append(" ~ ").AppendLine(FormatCoord(header.MaxX));
            report.Append("Y ").Append(FormatCoord(header.MinY)).Append(" ~ ").AppendLine(FormatCoord(header.MaxY));
            report.Append("Z ").Append(FormatCoord(header.MinZ)).Append(" ~ ").AppendLine(FormatCoord(header.MaxZ));
            report.Append("좌표계: ").AppendLine(ShortCrs(header.CrsWkt));
            report.Append("격자 ").Append(grid.Width.ToString("N0", CultureInfo.CurrentCulture));
            report.Append(" x ").Append(grid.Height.ToString("N0", CultureInfo.CurrentCulture));
            report.Append(", 칸 ").Append(grid.CellSizeMeters.ToString("0.##", CultureInfo.CurrentCulture));
            report.Append("m, ").Append(grid.ElevationRule);
            report.Append(", 사용 점 ").Append(grid.PointsUsed.ToString("N0", CultureInfo.CurrentCulture)).AppendLine("개");

            if (grid.CellSizeMeters > GridCellSizeMeters + 1e-6)
            {
                report.Append("파일 범위가 넓어 격자 크기를 ")
                    .Append(grid.CellSizeMeters.ToString("0.##", CultureInfo.CurrentCulture))
                    .AppendLine("m로 키웠습니다.");
            }

            if (grid.OutOfBoundsCount > 0)
            {
                report.Append("헤더 범위 밖 점 ")
                    .Append(grid.OutOfBoundsCount.ToString("N0", CultureInfo.CurrentCulture))
                    .AppendLine("개는 제외했습니다.");
            }

            if (input.HasStart)
                AppendSample(report, "시점", input.X1, input.Y1, grid.GetElevation(input.X1, input.Y1, input.Radius), input.Radius);

            if (input.HasEnd)
                AppendSample(report, "종점", input.X2, input.Y2, grid.GetElevation(input.X2, input.Y2, input.Radius), input.Radius);

            return report.ToString();
        }

        private static void AppendSample(StringBuilder report, string name, double x, double y, ElevationSample? sample, double radius)
        {
            report.AppendLine();
            report.Append(name).Append(" (").Append(FormatCoord(x)).Append(", ").Append(FormatCoord(y)).AppendLine(")");
            if (sample is null)
            {
                report.Append("  반경 ").Append(radius.ToString("0.##", CultureInfo.CurrentCulture)).AppendLine("m 안에 고도가 없습니다.");
                return;
            }

            report.Append("  Z = ").Append(sample.Value.Z.ToString("0.000", CultureInfo.CurrentCulture)).AppendLine(" m");
            report.Append("  반경 ").Append(radius.ToString("0.##", CultureInfo.CurrentCulture));
            report.Append("m, 격자 ").Append(sample.Value.CellCount).AppendLine("칸 거리 가중 평균");
        }

        private bool TryReadInputs(out QueryInput input)
        {
            input = default;
            bool hasStart = HasText(textBox_x1) || HasText(textBox_y1);
            bool hasEnd = HasText(textBox_x2) || HasText(textBox_y2);
            if (!hasStart && !hasEnd)
            {
                MessageBox.Show(this, "시점 또는 종점 좌표를 입력하세요.", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            if (hasStart && (!TryParseNumber(textBox_x1.Text, out x1) || !TryParseNumber(textBox_y1.Text, out y1)))
            {
                MessageBox.Show(this, "시점 X/Y를 숫자로 입력하세요.", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (hasEnd && (!TryParseNumber(textBox_x2.Text, out x2) || !TryParseNumber(textBox_y2.Text, out y2)))
            {
                MessageBox.Show(this, "종점 X/Y를 숫자로 입력하세요.", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!TryParseNumber(textBox_radius.Text, out double radius) || radius <= 0 || radius > MaxRadiusMeters)
            {
                MessageBox.Show(this, "검색 반경(m)은 0보다 크고 200 이하인 숫자로 입력하세요.", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            input = new QueryInput(hasStart, x1, y1, hasEnd, x2, y2, radius);
            return true;
        }

        private bool IsCurrentGrid(string path)
        {
            if (_grid == null || _loadedPath == null || !File.Exists(path))
                return false;

            var info = new FileInfo(path);
            return string.Equals(_loadedPath, info.FullName, StringComparison.OrdinalIgnoreCase)
                && info.Length == _loadedLength
                && info.LastWriteTimeUtc == _loadedWriteTimeUtc;
        }

        private void SetBusy(bool busy)
        {
            buttonQueryZ.Enabled = !busy;
            buttonOpenLas.Enabled = !busy;
            button1.Enabled = !busy;
            buttonPickStart.Enabled = !busy;
            buttonPickEnd.Enabled = !busy;
            UseWaitCursor = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private static bool HasText(TextBox box) => !string.IsNullOrWhiteSpace(box.Text);

        private static bool TryParseNumber(string text, out double value)
        {
            text = text.Trim();
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return double.IsFinite(value);
            }

            value = 0;
            return false;
        }

        private static string FormatCoord(double value) => value.ToString("0.###", CultureInfo.CurrentCulture);

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1L << 30)
                return $"{bytes / 1024d / 1024d / 1024d:0.00} GB";
            if (bytes >= 1L << 20)
                return $"{bytes / 1024d / 1024d:0.0} MB";
            return $"{bytes / 1024d:0.0} KB";
        }

        private static string ShortCrs(string? wkt)
        {
            if (string.IsNullOrWhiteSpace(wkt))
                return "없음";

            string oneLine = wkt.Replace("\r", " ").Replace("\n", " ").Trim();
            return oneLine.Length <= 160 ? oneLine : oneLine[..160] + "...";
        }

        private readonly record struct QueryInput(
            bool HasStart,
            double X1,
            double Y1,
            bool HasEnd,
            double X2,
            double Y2,
            double Radius);
    }
}
