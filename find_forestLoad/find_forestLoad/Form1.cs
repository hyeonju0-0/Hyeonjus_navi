using System.Globalization;
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

        public Form1()
        {
            InitializeComponent();
            AcceptButton = buttonQueryZ;
            textBox_radius.Text = "2";
            buttonOpenLas.Click += buttonOpenLas_Click;
            buttonQueryZ.Click += buttonQueryZ_Click;
            buttonPickStart.Click += (_, _) => PickFromAutoCad(textBox_x1, textBox_y1, "시점을 클릭하세요");
            buttonPickEnd.Click += (_, _) => PickFromAutoCad(textBox_x2, textBox_y2, "종점을 클릭하세요");
            button1.Click += buttonRoad_Click;
            FormClosing += Form1_FormClosing;
            textBox_result.Text =
                "LAS 파일을 연 다음, 시점과 종점의 X/Y를 입력하고 고도 조회를 누르세요." + Environment.NewLine +
                "AutoCAD에 DWG를 열어 두면 시점을 CAD에서, 종점을 CAD에서 버튼으로 점을 찍을 수 있습니다." + Environment.NewLine +
                "허용 경사도(%)를 입력하고 임도 생성을 누르면 시점, 중간점, 종점의 경사를 계산합니다." + Environment.NewLine +
                "첫 조회에서 파일 전체를 한 번 읽습니다. 2GB를 넘으면 몇 분 걸릴 수 있습니다.";
        }

        private void PickFromAutoCad(TextBox xBox, TextBox yBox, string prompt)
        {
            if (_busy)
                return;

            textBox_result.Text = "AutoCAD 창으로 이동합니다. 도면에서 점을 클릭하세요. Esc로 취소합니다.";
            Refresh();

            AutoCadPickResult result = AutoCadPointPicker.TryPick(prompt);
            Activate();

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

            if (!TryReadRoadInput(out double x1, out double y1, out double x2, out double y2, out double radius, out double slopePercent))
                return;

            _busy = true;
            SetBusy(true);
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                ElevationGrid? grid = await LoadGridAsync();
                if (IsDisposed || grid == null)
                    return;

                textBox_result.Text = "허용 경사 안에서 임도 경로를 찾는 중입니다.";
                var progress = new Progress<ElevationBuildProgress>(OnBuildProgress);
                ForestRoadResult result = await Task.Run(
                    () => grid.FindRoad(x1, y1, x2, y2, radius, slopePercent, progress, _cts.Token),
                    _cts.Token);

                if (IsDisposed)
                    return;

                if (!result.Succeeded || result.Path == null)
                {
                    _road = null;
                    progressBar1.Value = 0;
                    textBox_result.Text = result.Failure ?? "임도 경로를 찾지 못했습니다.";
                    return;
                }

                _road = result.Path;
                progressBar1.Value = progressBar1.Maximum;
                textBox_z1.Text = _road.Vertices[0].Z.ToString("0.000", CultureInfo.CurrentCulture);
                textBox_z2.Text = _road.Vertices[^1].Z.ToString("0.000", CultureInfo.CurrentCulture);
                textBox_result.Text = DescribeRoad(_road, slopePercent);
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
            out double radius,
            out double slopePercent)
        {
            x1 = y1 = x2 = y2 = radius = slopePercent = 0;
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

            if (!TryParseNumber(textBox_slope.Text, out slopePercent) || slopePercent <= 0 || slopePercent > 100)
            {
                MessageBox.Show(this, "허용 경사도를 0보다 크고 100 이하인 %로 입력하세요. 예: 10", "입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        private static string DescribeRoad(ForestRoadPath road, double allowedSlopePercent)
        {
            var report = new StringBuilder();
            report.Append("임도 경로 (A*)  허용 경사 ").Append(allowedSlopePercent.ToString("0.##", CultureInfo.CurrentCulture)).AppendLine("%");
            report.Append("수평 길이 ").Append(road.LengthMeters.ToString("0.0", CultureInfo.CurrentCulture)).Append(" m");
            report.Append("  |  구간 최대 경사 ").Append(road.MaxSlopePercent.ToString("0.0", CultureInfo.CurrentCulture)).Append('%');
            report.Append("  |  칸 사이 최대 경사 ").Append(road.MaxStepSlopePercent.ToString("0.0", CultureInfo.CurrentCulture)).AppendLine("%");
            int middle = Math.Max(0, road.Vertices.Count - 2);
            report.Append("꼭짓점 ").Append(road.Vertices.Count).Append("개 (시점, 중간점 ").Append(middle).AppendLine("개, 종점)");
            report.AppendLine("CAD에는 아직 그리지 않습니다.");

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
