namespace find_forestLoad
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            layoutRoot = new TableLayoutPanel();
            layoutToolbar = new TableLayoutPanel();
            flowButtons = new FlowLayoutPanel();
            buttonOpenLas = new Button();
            buttonQueryZ = new Button();
            button1 = new Button();
            buttonPreviewCad = new Button();
            labelLasFile = new Label();
            layoutBody = new TableLayoutPanel();
            groupStandard = new GroupBox();
            layoutStandard = new TableLayoutPanel();
            labelKind = new Label();
            comboRoadKind = new ComboBox();
            labelTerrain = new Label();
            comboTerrain = new ComboBox();
            labelSpeed = new Label();
            comboDesignSpeed = new ComboBox();
            label5 = new Label();
            layoutGrade = new TableLayoutPanel();
            textBox_slope = new NumericUpDown();
            buttonResetGrade = new Button();
            panelSummary = new Panel();
            layoutSummary = new TableLayoutPanel();
            labelSummaryGrade = new Label();
            labelSummaryGradeValue = new Label();
            labelSummaryWidth = new Label();
            labelSummaryWidthValue = new Label();
            labelSummaryCurve = new Label();
            labelSummaryCurveValue = new Label();
            label7 = new Label();
            layoutRight = new TableLayoutPanel();
            groupSection = new GroupBox();
            layoutSection = new TableLayoutPanel();
            label2 = new Label();
            label3 = new Label();
            label6 = new Label();
            panelStart = new TableLayoutPanel();
            labelDotStart = new Label();
            label1 = new Label();
            textBox_x1 = new TextBox();
            textBox_y1 = new TextBox();
            textBox_z1 = new TextBox();
            buttonPickStart = new Button();
            panelEnd = new TableLayoutPanel();
            labelDotEnd = new Label();
            label4 = new Label();
            textBox_x2 = new TextBox();
            textBox_y2 = new TextBox();
            textBox_z2 = new TextBox();
            buttonPickEnd = new Button();
            layoutRadius = new TableLayoutPanel();
            label8 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            textBox_radius = new TextBox();
            groupLog = new GroupBox();
            textBox_result = new TextBox();
            layoutStatus = new TableLayoutPanel();
            labelStatus = new Label();
            progressBar1 = new ProgressBar();
            labelProgressPercent = new Label();
            layoutRoot.SuspendLayout();
            layoutToolbar.SuspendLayout();
            flowButtons.SuspendLayout();
            layoutBody.SuspendLayout();
            groupStandard.SuspendLayout();
            layoutStandard.SuspendLayout();
            layoutGrade.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)textBox_slope).BeginInit();
            panelSummary.SuspendLayout();
            layoutSummary.SuspendLayout();
            layoutRight.SuspendLayout();
            groupSection.SuspendLayout();
            layoutSection.SuspendLayout();
            panelStart.SuspendLayout();
            panelEnd.SuspendLayout();
            layoutRadius.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupLog.SuspendLayout();
            layoutStatus.SuspendLayout();
            SuspendLayout();
            // 
            // layoutRoot
            // 
            layoutRoot.ColumnCount = 1;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.Controls.Add(layoutToolbar, 0, 0);
            layoutRoot.Controls.Add(layoutBody, 0, 1);
            layoutRoot.Controls.Add(layoutStatus, 0, 2);
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Location = new Point(0, 0);
            layoutRoot.Name = "layoutRoot";
            layoutRoot.RowCount = 3;
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layoutRoot.Size = new Size(1024, 640);
            layoutRoot.TabIndex = 0;
            // 
            // layoutToolbar
            // 
            layoutToolbar.ColumnCount = 2;
            layoutToolbar.ColumnStyles.Add(new ColumnStyle());
            layoutToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutToolbar.Controls.Add(flowButtons, 0, 0);
            layoutToolbar.Controls.Add(labelLasFile, 1, 0);
            layoutToolbar.Dock = DockStyle.Fill;
            layoutToolbar.Location = new Point(0, 0);
            layoutToolbar.Margin = new Padding(0);
            layoutToolbar.Name = "layoutToolbar";
            layoutToolbar.Padding = new Padding(12, 0, 12, 0);
            layoutToolbar.RowCount = 1;
            layoutToolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutToolbar.Size = new Size(1024, 52);
            layoutToolbar.TabIndex = 0;
            // 
            // flowButtons
            // 
            flowButtons.AutoSize = true;
            flowButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowButtons.Controls.Add(buttonOpenLas);
            flowButtons.Controls.Add(buttonQueryZ);
            flowButtons.Controls.Add(button1);
            flowButtons.Controls.Add(buttonPreviewCad);
            flowButtons.Dock = DockStyle.Bottom;
            flowButtons.Location = new Point(12, 12);
            flowButtons.Margin = new Padding(0);
            flowButtons.Name = "flowButtons";
            flowButtons.Size = new Size(506, 40);
            flowButtons.TabIndex = 0;
            flowButtons.WrapContents = false;
            // 
            // buttonOpenLas
            // 
            buttonOpenLas.AutoSize = true;
            buttonOpenLas.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOpenLas.BackColor = Color.White;
            buttonOpenLas.Font = new Font("맑은 고딕", 9.75F);
            buttonOpenLas.Location = new Point(0, 0);
            buttonOpenLas.Margin = new Padding(0, 0, 8, 0);
            buttonOpenLas.MinimumSize = new Size(88, 40);
            buttonOpenLas.Name = "buttonOpenLas";
            buttonOpenLas.Padding = new Padding(10, 0, 10, 0);
            buttonOpenLas.Size = new Size(91, 40);
            buttonOpenLas.TabIndex = 0;
            buttonOpenLas.Text = "LAS 열기";
            buttonOpenLas.UseVisualStyleBackColor = false;
            // 
            // buttonQueryZ
            // 
            buttonQueryZ.AutoSize = true;
            buttonQueryZ.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonQueryZ.Font = new Font("맑은 고딕", 9.75F);
            buttonQueryZ.Location = new Point(99, 0);
            buttonQueryZ.Margin = new Padding(0, 0, 8, 0);
            buttonQueryZ.MinimumSize = new Size(88, 40);
            buttonQueryZ.Name = "buttonQueryZ";
            buttonQueryZ.Padding = new Padding(10, 0, 10, 0);
            buttonQueryZ.Size = new Size(95, 40);
            buttonQueryZ.TabIndex = 1;
            buttonQueryZ.Text = "고도 조회";
            buttonQueryZ.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button1.Font = new Font("맑은 고딕", 9.75F);
            button1.Location = new Point(202, 0);
            button1.Margin = new Padding(0, 0, 8, 0);
            button1.MinimumSize = new Size(88, 40);
            button1.Name = "button1";
            button1.Padding = new Padding(10, 0, 10, 0);
            button1.Size = new Size(95, 40);
            button1.TabIndex = 2;
            button1.Text = "임도 생성";
            button1.UseVisualStyleBackColor = true;
            // 
            // buttonPreviewCad
            // 
            buttonPreviewCad.AutoSize = true;
            buttonPreviewCad.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonPreviewCad.Font = new Font("맑은 고딕", 9.75F);
            buttonPreviewCad.Location = new Point(305, 0);
            buttonPreviewCad.Margin = new Padding(0);
            buttonPreviewCad.MinimumSize = new Size(88, 40);
            buttonPreviewCad.Name = "buttonPreviewCad";
            buttonPreviewCad.Padding = new Padding(10, 0, 10, 0);
            buttonPreviewCad.Size = new Size(201, 40);
            buttonPreviewCad.TabIndex = 3;
            buttonPreviewCad.Text = "노선 선택 후 CAD에서 보기";
            buttonPreviewCad.UseVisualStyleBackColor = true;
            // 
            // labelLasFile
            // 
            labelLasFile.AutoEllipsis = true;
            labelLasFile.Dock = DockStyle.Fill;
            labelLasFile.ForeColor = Color.FromArgb(96, 96, 96);
            labelLasFile.Location = new Point(530, 0);
            labelLasFile.Margin = new Padding(12, 0, 0, 0);
            labelLasFile.Name = "labelLasFile";
            labelLasFile.Size = new Size(482, 52);
            labelLasFile.TabIndex = 1;
            labelLasFile.Text = "선택된 파일 없음";
            labelLasFile.TextAlign = ContentAlignment.MiddleRight;
            // 
            // layoutBody
            // 
            layoutBody.ColumnCount = 2;
            layoutBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 292F));
            layoutBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutBody.Controls.Add(groupStandard, 0, 0);
            layoutBody.Controls.Add(layoutRight, 1, 0);
            layoutBody.Dock = DockStyle.Fill;
            layoutBody.Location = new Point(0, 52);
            layoutBody.Margin = new Padding(0);
            layoutBody.Name = "layoutBody";
            layoutBody.Padding = new Padding(12, 0, 12, 4);
            layoutBody.RowCount = 1;
            layoutBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutBody.Size = new Size(1024, 552);
            layoutBody.TabIndex = 1;
            // 
            // groupStandard
            // 
            groupStandard.Controls.Add(layoutStandard);
            groupStandard.Dock = DockStyle.Fill;
            groupStandard.Location = new Point(15, 3);
            groupStandard.Margin = new Padding(3, 3, 6, 3);
            groupStandard.Name = "groupStandard";
            groupStandard.Padding = new Padding(12, 8, 12, 10);
            groupStandard.Size = new Size(283, 542);
            groupStandard.TabIndex = 0;
            groupStandard.TabStop = false;
            groupStandard.Text = "적용 기준";
            // 
            // layoutStandard
            // 
            layoutStandard.ColumnCount = 1;
            layoutStandard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutStandard.Controls.Add(labelKind, 0, 0);
            layoutStandard.Controls.Add(comboRoadKind, 0, 1);
            layoutStandard.Controls.Add(labelTerrain, 0, 2);
            layoutStandard.Controls.Add(comboTerrain, 0, 3);
            layoutStandard.Controls.Add(labelSpeed, 0, 4);
            layoutStandard.Controls.Add(comboDesignSpeed, 0, 5);
            layoutStandard.Controls.Add(label5, 0, 6);
            layoutStandard.Controls.Add(layoutGrade, 0, 7);
            layoutStandard.Controls.Add(panelSummary, 0, 8);
            layoutStandard.Controls.Add(label7, 0, 9);
            layoutStandard.Dock = DockStyle.Fill;
            layoutStandard.Location = new Point(12, 24);
            layoutStandard.Margin = new Padding(0);
            layoutStandard.Name = "layoutStandard";
            layoutStandard.RowCount = 10;
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
            layoutStandard.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutStandard.Size = new Size(259, 508);
            layoutStandard.TabIndex = 0;
            // 
            // labelKind
            // 
            labelKind.Dock = DockStyle.Fill;
            labelKind.Font = new Font("맑은 고딕", 9.75F);
            labelKind.Location = new Point(3, 0);
            labelKind.Name = "labelKind";
            labelKind.Size = new Size(253, 22);
            labelKind.TabIndex = 0;
            labelKind.Text = "임도 종류";
            labelKind.TextAlign = ContentAlignment.BottomLeft;
            // 
            // comboRoadKind
            // 
            comboRoadKind.BackColor = SystemColors.Control;
            comboRoadKind.Dock = DockStyle.Fill;
            comboRoadKind.DropDownStyle = ComboBoxStyle.DropDownList;
            comboRoadKind.Font = new Font("맑은 고딕", 11.25F);
            comboRoadKind.FormattingEnabled = true;
            comboRoadKind.Items.AddRange(new object[] { "간선임도", "산불진화임도", "작업임도" });
            comboRoadKind.Location = new Point(3, 25);
            comboRoadKind.Name = "comboRoadKind";
            comboRoadKind.Size = new Size(253, 28);
            comboRoadKind.TabIndex = 1;
            // 
            // labelTerrain
            // 
            labelTerrain.Dock = DockStyle.Fill;
            labelTerrain.Font = new Font("맑은 고딕", 9.75F);
            labelTerrain.Location = new Point(3, 54);
            labelTerrain.Name = "labelTerrain";
            labelTerrain.Size = new Size(253, 22);
            labelTerrain.TabIndex = 2;
            labelTerrain.Text = "지형";
            labelTerrain.TextAlign = ContentAlignment.BottomLeft;
            // 
            // comboTerrain
            // 
            comboTerrain.BackColor = SystemColors.Control;
            comboTerrain.Dock = DockStyle.Fill;
            comboTerrain.DropDownStyle = ComboBoxStyle.DropDownList;
            comboTerrain.Font = new Font("맑은 고딕", 11.25F);
            comboTerrain.FormattingEnabled = true;
            comboTerrain.Items.AddRange(new object[] { "일반지형", "특수지형" });
            comboTerrain.Location = new Point(3, 79);
            comboTerrain.Name = "comboTerrain";
            comboTerrain.Size = new Size(253, 28);
            comboTerrain.TabIndex = 3;
            // 
            // labelSpeed
            // 
            labelSpeed.Dock = DockStyle.Fill;
            labelSpeed.Font = new Font("맑은 고딕", 9.75F);
            labelSpeed.Location = new Point(3, 108);
            labelSpeed.Name = "labelSpeed";
            labelSpeed.Size = new Size(253, 22);
            labelSpeed.TabIndex = 4;
            labelSpeed.Text = "설계속도";
            labelSpeed.TextAlign = ContentAlignment.BottomLeft;
            // 
            // comboDesignSpeed
            // 
            comboDesignSpeed.BackColor = SystemColors.Control;
            comboDesignSpeed.Dock = DockStyle.Fill;
            comboDesignSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            comboDesignSpeed.Font = new Font("맑은 고딕", 11.25F);
            comboDesignSpeed.FormattingEnabled = true;
            comboDesignSpeed.Items.AddRange(new object[] { "20", "30", "40" });
            comboDesignSpeed.Location = new Point(3, 133);
            comboDesignSpeed.Name = "comboDesignSpeed";
            comboDesignSpeed.Size = new Size(253, 28);
            comboDesignSpeed.TabIndex = 5;
            // 
            // label5
            // 
            label5.Dock = DockStyle.Fill;
            label5.Font = new Font("맑은 고딕", 9.75F);
            label5.Location = new Point(3, 162);
            label5.Name = "label5";
            label5.Size = new Size(253, 22);
            label5.TabIndex = 6;
            label5.Text = "허용 경사 (%)";
            label5.TextAlign = ContentAlignment.BottomLeft;
            // 
            // layoutGrade
            // 
            layoutGrade.ColumnCount = 2;
            layoutGrade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutGrade.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84F));
            layoutGrade.Controls.Add(textBox_slope, 0, 0);
            layoutGrade.Controls.Add(buttonResetGrade, 1, 0);
            layoutGrade.Dock = DockStyle.Fill;
            layoutGrade.Location = new Point(0, 184);
            layoutGrade.Margin = new Padding(0);
            layoutGrade.Name = "layoutGrade";
            layoutGrade.RowCount = 1;
            layoutGrade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutGrade.Size = new Size(259, 34);
            layoutGrade.TabIndex = 7;
            // 
            // textBox_slope
            // 
            textBox_slope.BackColor = SystemColors.Control;
            textBox_slope.DecimalPlaces = 1;
            textBox_slope.Dock = DockStyle.Fill;
            textBox_slope.Font = new Font("맑은 고딕", 11.25F);
            textBox_slope.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            textBox_slope.Location = new Point(3, 5);
            textBox_slope.Margin = new Padding(3, 5, 3, 3);
            textBox_slope.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            textBox_slope.Name = "textBox_slope";
            textBox_slope.Size = new Size(169, 27);
            textBox_slope.TabIndex = 0;
            textBox_slope.TextAlign = HorizontalAlignment.Center;
            textBox_slope.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // buttonResetGrade
            // 
            buttonResetGrade.Dock = DockStyle.Fill;
            buttonResetGrade.Font = new Font("맑은 고딕", 11.25F);
            buttonResetGrade.Location = new Point(183, 3);
            buttonResetGrade.Margin = new Padding(8, 3, 3, 3);
            buttonResetGrade.Name = "buttonResetGrade";
            buttonResetGrade.Size = new Size(73, 28);
            buttonResetGrade.TabIndex = 1;
            buttonResetGrade.Text = "기준값";
            buttonResetGrade.UseVisualStyleBackColor = true;
            // 
            // panelSummary
            // 
            panelSummary.BackColor = SystemColors.Control;
            panelSummary.BorderStyle = BorderStyle.FixedSingle;
            panelSummary.Controls.Add(layoutSummary);
            panelSummary.Dock = DockStyle.Fill;
            panelSummary.Location = new Point(3, 226);
            panelSummary.Margin = new Padding(3, 8, 3, 3);
            panelSummary.Name = "panelSummary";
            panelSummary.Padding = new Padding(10, 6, 10, 6);
            panelSummary.Size = new Size(253, 93);
            panelSummary.TabIndex = 8;
            // 
            // layoutSummary
            // 
            layoutSummary.ColumnCount = 2;
            layoutSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            layoutSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            layoutSummary.Controls.Add(labelSummaryGrade, 0, 0);
            layoutSummary.Controls.Add(labelSummaryGradeValue, 1, 0);
            layoutSummary.Controls.Add(labelSummaryWidth, 0, 1);
            layoutSummary.Controls.Add(labelSummaryWidthValue, 1, 1);
            layoutSummary.Controls.Add(labelSummaryCurve, 0, 2);
            layoutSummary.Controls.Add(labelSummaryCurveValue, 1, 2);
            layoutSummary.Dock = DockStyle.Fill;
            layoutSummary.Location = new Point(10, 6);
            layoutSummary.Margin = new Padding(0);
            layoutSummary.Name = "layoutSummary";
            layoutSummary.RowCount = 3;
            layoutSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            layoutSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            layoutSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
            layoutSummary.Size = new Size(231, 79);
            layoutSummary.TabIndex = 0;
            // 
            // labelSummaryGrade
            // 
            labelSummaryGrade.Dock = DockStyle.Fill;
            labelSummaryGrade.Font = new Font("맑은 고딕", 9.75F);
            labelSummaryGrade.Location = new Point(3, 0);
            labelSummaryGrade.Name = "labelSummaryGrade";
            labelSummaryGrade.Size = new Size(104, 26);
            labelSummaryGrade.TabIndex = 0;
            labelSummaryGrade.Text = "종단경사 한도";
            labelSummaryGrade.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelSummaryGradeValue
            // 
            labelSummaryGradeValue.Dock = DockStyle.Fill;
            labelSummaryGradeValue.Font = new Font("맑은 고딕", 9.75F);
            labelSummaryGradeValue.Location = new Point(113, 0);
            labelSummaryGradeValue.Name = "labelSummaryGradeValue";
            labelSummaryGradeValue.Size = new Size(115, 26);
            labelSummaryGradeValue.TabIndex = 1;
            labelSummaryGradeValue.Text = "9% (기준표)";
            labelSummaryGradeValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // labelSummaryWidth
            // 
            labelSummaryWidth.Dock = DockStyle.Fill;
            labelSummaryWidth.Font = new Font("맑은 고딕", 9.75F);
            labelSummaryWidth.Location = new Point(3, 26);
            labelSummaryWidth.Name = "labelSummaryWidth";
            labelSummaryWidth.Size = new Size(104, 26);
            labelSummaryWidth.TabIndex = 2;
            labelSummaryWidth.Text = "유효너비 기준";
            labelSummaryWidth.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelSummaryWidthValue
            // 
            labelSummaryWidthValue.Dock = DockStyle.Fill;
            labelSummaryWidthValue.Font = new Font("맑은 고딕", 9.75F);
            labelSummaryWidthValue.Location = new Point(113, 26);
            labelSummaryWidthValue.Name = "labelSummaryWidthValue";
            labelSummaryWidthValue.Size = new Size(115, 26);
            labelSummaryWidthValue.TabIndex = 3;
            labelSummaryWidthValue.Text = "3m";
            labelSummaryWidthValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // labelSummaryCurve
            // 
            labelSummaryCurve.Dock = DockStyle.Fill;
            labelSummaryCurve.Font = new Font("맑은 고딕", 9.75F);
            labelSummaryCurve.Location = new Point(3, 52);
            labelSummaryCurve.Name = "labelSummaryCurve";
            labelSummaryCurve.Size = new Size(104, 27);
            labelSummaryCurve.TabIndex = 4;
            labelSummaryCurve.Text = "배향곡선지 너비";
            labelSummaryCurve.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelSummaryCurveValue
            // 
            labelSummaryCurveValue.Dock = DockStyle.Fill;
            labelSummaryCurveValue.Font = new Font("맑은 고딕", 9.75F);
            labelSummaryCurveValue.Location = new Point(113, 52);
            labelSummaryCurveValue.Name = "labelSummaryCurveValue";
            labelSummaryCurveValue.Size = new Size(115, 27);
            labelSummaryCurveValue.TabIndex = 5;
            labelSummaryCurveValue.Text = "6m 이상";
            labelSummaryCurveValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            label7.Dock = DockStyle.Fill;
            label7.ForeColor = Color.FromArgb(90, 90, 90);
            label7.Location = new Point(3, 322);
            label7.Name = "label7";
            label7.Size = new Size(253, 186);
            label7.TabIndex = 9;
            label7.Text = "경사 칸에 값을 넣으면 그 한도로 탐색합니다. 기준값은 표 값으로 되돌립니다. 현재 A*는 경사만 탐색에 반영하며, 도로 폭·곡선·배수는 다음 단계에서 검토합니다.";
            label7.TextAlign = ContentAlignment.BottomLeft;
            // 
            // layoutRight
            // 
            layoutRight.ColumnCount = 1;
            layoutRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRight.Controls.Add(groupSection, 0, 0);
            layoutRight.Controls.Add(groupLog, 0, 1);
            layoutRight.Dock = DockStyle.Fill;
            layoutRight.Location = new Point(304, 0);
            layoutRight.Margin = new Padding(0);
            layoutRight.Name = "layoutRight";
            layoutRight.RowCount = 2;
            layoutRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 214F));
            layoutRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRight.Size = new Size(708, 548);
            layoutRight.TabIndex = 1;
            // 
            // groupSection
            // 
            groupSection.Controls.Add(layoutSection);
            groupSection.Dock = DockStyle.Fill;
            groupSection.Location = new Point(6, 3);
            groupSection.Margin = new Padding(6, 3, 3, 4);
            groupSection.Name = "groupSection";
            groupSection.Padding = new Padding(12, 8, 12, 10);
            groupSection.Size = new Size(699, 207);
            groupSection.TabIndex = 0;
            groupSection.TabStop = false;
            groupSection.Text = "구간 설정";
            // 
            // layoutSection
            // 
            layoutSection.ColumnCount = 5;
            layoutSection.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78F));
            layoutSection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            layoutSection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            layoutSection.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            layoutSection.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128F));
            layoutSection.Controls.Add(label2, 1, 0);
            layoutSection.Controls.Add(label3, 2, 0);
            layoutSection.Controls.Add(label6, 3, 0);
            layoutSection.Controls.Add(panelStart, 0, 1);
            layoutSection.Controls.Add(textBox_x1, 1, 1);
            layoutSection.Controls.Add(textBox_y1, 2, 1);
            layoutSection.Controls.Add(textBox_z1, 3, 1);
            layoutSection.Controls.Add(buttonPickStart, 4, 1);
            layoutSection.Controls.Add(panelEnd, 0, 2);
            layoutSection.Controls.Add(textBox_x2, 1, 2);
            layoutSection.Controls.Add(textBox_y2, 2, 2);
            layoutSection.Controls.Add(textBox_z2, 3, 2);
            layoutSection.Controls.Add(buttonPickEnd, 4, 2);
            layoutSection.Controls.Add(layoutRadius, 0, 3);
            layoutSection.Dock = DockStyle.Fill;
            layoutSection.Location = new Point(12, 24);
            layoutSection.Name = "layoutSection";
            layoutSection.RowCount = 4;
            layoutSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            layoutSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layoutSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layoutSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layoutSection.Size = new Size(675, 173);
            layoutSection.TabIndex = 0;
            // 
            // label2
            // 
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("맑은 고딕", 9.75F);
            label2.ForeColor = Color.FromArgb(90, 90, 90);
            label2.Location = new Point(81, 0);
            label2.Name = "label2";
            label2.Size = new Size(184, 26);
            label2.TabIndex = 0;
            label2.Text = "X 좌표";
            label2.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label3
            // 
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("맑은 고딕", 9.75F);
            label3.ForeColor = Color.FromArgb(90, 90, 90);
            label3.Location = new Point(271, 0);
            label3.Name = "label3";
            label3.Size = new Size(184, 26);
            label3.TabIndex = 1;
            label3.Text = "Y 좌표";
            label3.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label6
            // 
            label6.Dock = DockStyle.Fill;
            label6.Font = new Font("맑은 고딕", 9.75F);
            label6.ForeColor = Color.FromArgb(90, 90, 90);
            label6.Location = new Point(461, 0);
            label6.Name = "label6";
            label6.Size = new Size(82, 26);
            label6.TabIndex = 2;
            label6.Text = "고도 (m)";
            label6.TextAlign = ContentAlignment.BottomCenter;
            // 
            // panelStart
            // 
            panelStart.ColumnCount = 2;
            panelStart.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
            panelStart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelStart.Controls.Add(labelDotStart, 0, 0);
            panelStart.Controls.Add(label1, 1, 0);
            panelStart.Dock = DockStyle.Fill;
            panelStart.Location = new Point(0, 26);
            panelStart.Margin = new Padding(0);
            panelStart.Name = "panelStart";
            panelStart.RowCount = 1;
            panelStart.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelStart.Size = new Size(78, 40);
            panelStart.TabIndex = 3;
            // 
            // labelDotStart
            // 
            labelDotStart.Dock = DockStyle.Fill;
            labelDotStart.ForeColor = Color.Red;
            labelDotStart.Location = new Point(0, 0);
            labelDotStart.Margin = new Padding(0);
            labelDotStart.Name = "labelDotStart";
            labelDotStart.Size = new Size(18, 40);
            labelDotStart.TabIndex = 0;
            labelDotStart.Text = "●";
            labelDotStart.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("맑은 고딕", 9.75F);
            label1.Location = new Point(18, 0);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Size = new Size(60, 40);
            label1.TabIndex = 1;
            label1.Text = "시점";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBox_x1
            // 
            textBox_x1.BackColor = SystemColors.Control;
            textBox_x1.Dock = DockStyle.Fill;
            textBox_x1.Font = new Font("맑은 고딕", 11.25F);
            textBox_x1.Location = new Point(81, 34);
            textBox_x1.Margin = new Padding(3, 8, 3, 6);
            textBox_x1.Name = "textBox_x1";
            textBox_x1.Size = new Size(184, 27);
            textBox_x1.TabIndex = 4;
            // 
            // textBox_y1
            // 
            textBox_y1.BackColor = SystemColors.Control;
            textBox_y1.Dock = DockStyle.Fill;
            textBox_y1.Font = new Font("맑은 고딕", 11.25F);
            textBox_y1.Location = new Point(271, 34);
            textBox_y1.Margin = new Padding(3, 8, 3, 6);
            textBox_y1.Name = "textBox_y1";
            textBox_y1.Size = new Size(184, 27);
            textBox_y1.TabIndex = 5;
            // 
            // textBox_z1
            // 
            textBox_z1.BackColor = SystemColors.Control;
            textBox_z1.Dock = DockStyle.Fill;
            textBox_z1.Enabled = false;
            textBox_z1.Font = new Font("맑은 고딕", 11.25F);
            textBox_z1.Location = new Point(461, 34);
            textBox_z1.Margin = new Padding(3, 8, 3, 6);
            textBox_z1.Name = "textBox_z1";
            textBox_z1.PlaceholderText = "자동";
            textBox_z1.ReadOnly = true;
            textBox_z1.Size = new Size(82, 27);
            textBox_z1.TabIndex = 6;
            textBox_z1.TabStop = false;
            textBox_z1.TextAlign = HorizontalAlignment.Center;
            // 
            // buttonPickStart
            // 
            buttonPickStart.Dock = DockStyle.Fill;
            buttonPickStart.Font = new Font("맑은 고딕", 11.25F);
            buttonPickStart.Location = new Point(549, 31);
            buttonPickStart.Margin = new Padding(3, 5, 3, 5);
            buttonPickStart.Name = "buttonPickStart";
            buttonPickStart.Size = new Size(123, 30);
            buttonPickStart.TabIndex = 7;
            buttonPickStart.Text = "CAD에서 선택";
            buttonPickStart.UseVisualStyleBackColor = true;
            // 
            // panelEnd
            // 
            panelEnd.ColumnCount = 2;
            panelEnd.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
            panelEnd.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelEnd.Controls.Add(labelDotEnd, 0, 0);
            panelEnd.Controls.Add(label4, 1, 0);
            panelEnd.Dock = DockStyle.Fill;
            panelEnd.Location = new Point(0, 66);
            panelEnd.Margin = new Padding(0);
            panelEnd.Name = "panelEnd";
            panelEnd.RowCount = 1;
            panelEnd.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelEnd.Size = new Size(78, 40);
            panelEnd.TabIndex = 8;
            // 
            // labelDotEnd
            // 
            labelDotEnd.Dock = DockStyle.Fill;
            labelDotEnd.ForeColor = Color.Red;
            labelDotEnd.Location = new Point(0, 0);
            labelDotEnd.Margin = new Padding(0);
            labelDotEnd.Name = "labelDotEnd";
            labelDotEnd.Size = new Size(18, 40);
            labelDotEnd.TabIndex = 0;
            labelDotEnd.Text = "●";
            labelDotEnd.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.Dock = DockStyle.Fill;
            label4.Font = new Font("맑은 고딕", 9.75F);
            label4.Location = new Point(18, 0);
            label4.Margin = new Padding(0);
            label4.Name = "label4";
            label4.Size = new Size(60, 40);
            label4.TabIndex = 1;
            label4.Text = "종점";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBox_x2
            // 
            textBox_x2.BackColor = SystemColors.Control;
            textBox_x2.Dock = DockStyle.Fill;
            textBox_x2.Font = new Font("맑은 고딕", 11.25F);
            textBox_x2.Location = new Point(81, 74);
            textBox_x2.Margin = new Padding(3, 8, 3, 6);
            textBox_x2.Name = "textBox_x2";
            textBox_x2.Size = new Size(184, 27);
            textBox_x2.TabIndex = 9;
            // 
            // textBox_y2
            // 
            textBox_y2.BackColor = SystemColors.Control;
            textBox_y2.Dock = DockStyle.Fill;
            textBox_y2.Font = new Font("맑은 고딕", 11.25F);
            textBox_y2.Location = new Point(271, 74);
            textBox_y2.Margin = new Padding(3, 8, 3, 6);
            textBox_y2.Name = "textBox_y2";
            textBox_y2.Size = new Size(184, 27);
            textBox_y2.TabIndex = 10;
            // 
            // textBox_z2
            // 
            textBox_z2.BackColor = SystemColors.Control;
            textBox_z2.Dock = DockStyle.Fill;
            textBox_z2.Enabled = false;
            textBox_z2.Font = new Font("맑은 고딕", 11.25F);
            textBox_z2.Location = new Point(461, 74);
            textBox_z2.Margin = new Padding(3, 8, 3, 6);
            textBox_z2.Name = "textBox_z2";
            textBox_z2.PlaceholderText = "자동";
            textBox_z2.ReadOnly = true;
            textBox_z2.Size = new Size(82, 27);
            textBox_z2.TabIndex = 11;
            textBox_z2.TabStop = false;
            textBox_z2.TextAlign = HorizontalAlignment.Center;
            // 
            // buttonPickEnd
            // 
            buttonPickEnd.Dock = DockStyle.Fill;
            buttonPickEnd.Font = new Font("맑은 고딕", 11.25F);
            buttonPickEnd.Location = new Point(549, 71);
            buttonPickEnd.Margin = new Padding(3, 5, 3, 5);
            buttonPickEnd.Name = "buttonPickEnd";
            buttonPickEnd.Size = new Size(123, 30);
            buttonPickEnd.TabIndex = 12;
            buttonPickEnd.Text = "CAD에서 선택";
            buttonPickEnd.UseVisualStyleBackColor = true;
            // 
            // layoutRadius
            // 
            layoutRadius.ColumnCount = 2;
            layoutSection.SetColumnSpan(layoutRadius, 2);
            layoutRadius.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));
            layoutRadius.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
            layoutRadius.Controls.Add(label8, 0, 0);
            layoutRadius.Controls.Add(tableLayoutPanel1, 1, 0);
            layoutRadius.Dock = DockStyle.Left;
            layoutRadius.Location = new Point(0, 110);
            layoutRadius.Margin = new Padding(0, 4, 0, 0);
            layoutRadius.Name = "layoutRadius";
            layoutRadius.RowCount = 1;
            layoutRadius.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRadius.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layoutRadius.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layoutRadius.Size = new Size(180, 63);
            layoutRadius.TabIndex = 13;
            // 
            // label8
            // 
            label8.Dock = DockStyle.Fill;
            label8.Font = new Font("맑은 고딕", 9.75F);
            label8.Location = new Point(3, 0);
            label8.Name = "label8";
            label8.Size = new Size(102, 63);
            label8.TabIndex = 0;
            label8.Text = "검색 반경 (m)";
            label8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(textBox_radius, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(111, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 66.6666641F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 16.666666F));
            tableLayoutPanel1.Size = new Size(66, 57);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // textBox_radius
            // 
            textBox_radius.BackColor = SystemColors.Control;
            textBox_radius.Dock = DockStyle.Fill;
            textBox_radius.Font = new Font("맑은 고딕", 11.25F);
            textBox_radius.Location = new Point(3, 15);
            textBox_radius.Margin = new Padding(3, 6, 3, 6);
            textBox_radius.Name = "textBox_radius";
            textBox_radius.Size = new Size(60, 27);
            textBox_radius.TabIndex = 1;
            textBox_radius.Text = "2";
            textBox_radius.TextAlign = HorizontalAlignment.Center;
            // 
            // groupLog
            // 
            groupLog.Controls.Add(textBox_result);
            groupLog.Dock = DockStyle.Fill;
            groupLog.Location = new Point(6, 218);
            groupLog.Margin = new Padding(6, 4, 3, 3);
            groupLog.Name = "groupLog";
            groupLog.Padding = new Padding(12, 8, 12, 12);
            groupLog.Size = new Size(699, 327);
            groupLog.TabIndex = 1;
            groupLog.TabStop = false;
            groupLog.Text = "진행 로그";
            // 
            // textBox_result
            // 
            textBox_result.BackColor = Color.FromArgb(244, 220, 213);
            textBox_result.BorderStyle = BorderStyle.FixedSingle;
            textBox_result.Dock = DockStyle.Fill;
            textBox_result.Font = new Font("맑은 고딕", 9.75F);
            textBox_result.Location = new Point(12, 24);
            textBox_result.Margin = new Padding(0);
            textBox_result.Multiline = true;
            textBox_result.Name = "textBox_result";
            textBox_result.ReadOnly = true;
            textBox_result.ScrollBars = ScrollBars.Vertical;
            textBox_result.Size = new Size(675, 291);
            textBox_result.TabIndex = 0;
            textBox_result.Text = "진행 상황이 여기에 표시됩니다.";
            // 
            // layoutStatus
            // 
            layoutStatus.ColumnCount = 3;
            layoutStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            layoutStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
            layoutStatus.Controls.Add(labelStatus, 0, 0);
            layoutStatus.Controls.Add(progressBar1, 1, 0);
            layoutStatus.Controls.Add(labelProgressPercent, 2, 0);
            layoutStatus.Dock = DockStyle.Fill;
            layoutStatus.Location = new Point(0, 604);
            layoutStatus.Margin = new Padding(0);
            layoutStatus.Name = "layoutStatus";
            layoutStatus.Padding = new Padding(12, 4, 8, 6);
            layoutStatus.RowCount = 1;
            layoutStatus.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutStatus.Size = new Size(1024, 36);
            layoutStatus.TabIndex = 2;
            // 
            // labelStatus
            // 
            labelStatus.Dock = DockStyle.Fill;
            labelStatus.Location = new Point(15, 4);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(58, 26);
            labelStatus.TabIndex = 0;
            labelStatus.Text = "준비";
            labelStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar1
            // 
            progressBar1.Dock = DockStyle.Fill;
            progressBar1.Location = new Point(76, 8);
            progressBar1.Margin = new Padding(0, 4, 8, 4);
            progressBar1.Maximum = 1000;
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(884, 18);
            progressBar1.TabIndex = 1;
            // 
            // labelProgressPercent
            // 
            labelProgressPercent.Dock = DockStyle.Fill;
            labelProgressPercent.Location = new Point(971, 4);
            labelProgressPercent.Name = "labelProgressPercent";
            labelProgressPercent.Size = new Size(42, 26);
            labelProgressPercent.TabIndex = 2;
            labelProgressPercent.Text = "0%";
            labelProgressPercent.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(238, 203, 202);
            ClientSize = new Size(1024, 640);
            Controls.Add(layoutRoot);
            Font = new Font("맑은 고딕", 9F);
            MinimumSize = new Size(920, 620);
            Name = "Form1";
            Text = "현주의 길찾기";
            layoutRoot.ResumeLayout(false);
            layoutToolbar.ResumeLayout(false);
            layoutToolbar.PerformLayout();
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            layoutBody.ResumeLayout(false);
            groupStandard.ResumeLayout(false);
            layoutStandard.ResumeLayout(false);
            layoutGrade.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)textBox_slope).EndInit();
            panelSummary.ResumeLayout(false);
            layoutSummary.ResumeLayout(false);
            layoutRight.ResumeLayout(false);
            groupSection.ResumeLayout(false);
            layoutSection.ResumeLayout(false);
            layoutSection.PerformLayout();
            panelStart.ResumeLayout(false);
            panelEnd.ResumeLayout(false);
            layoutRadius.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            groupLog.ResumeLayout(false);
            groupLog.PerformLayout();
            layoutStatus.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox textBox_x1;
        private TextBox textBox_y1;
        private TextBox textBox_x2;
        private TextBox textBox_y2;
        private TextBox textBox_z1;
        private TextBox textBox_z2;
        private NumericUpDown textBox_slope;
        private TextBox textBox_radius;
        private TextBox textBox_result;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label labelLasFile;
        private Button button1;
        private Button buttonOpenLas;
        private Button buttonQueryZ;
        private Button buttonPickStart;
        private Button buttonPickEnd;
        private Button buttonPreviewCad;
        private ProgressBar progressBar1;
        private TableLayoutPanel layoutRoot;
        private TableLayoutPanel layoutToolbar;
        private FlowLayoutPanel flowButtons;
        private TableLayoutPanel layoutBody;
        private GroupBox groupStandard;
        private TableLayoutPanel layoutStandard;
        private Label labelKind;
        private ComboBox comboRoadKind;
        private Label labelTerrain;
        private ComboBox comboTerrain;
        private Label labelSpeed;
        private ComboBox comboDesignSpeed;
        private TableLayoutPanel layoutGrade;
        private Button buttonResetGrade;
        private Panel panelSummary;
        private TableLayoutPanel layoutSummary;
        private Label labelSummaryGrade;
        private Label labelSummaryGradeValue;
        private Label labelSummaryWidth;
        private Label labelSummaryWidthValue;
        private Label labelSummaryCurve;
        private Label labelSummaryCurveValue;
        private TableLayoutPanel layoutRight;
        private GroupBox groupSection;
        private TableLayoutPanel layoutSection;
        private TableLayoutPanel panelStart;
        private Label labelDotStart;
        private TableLayoutPanel panelEnd;
        private Label labelDotEnd;
        private TableLayoutPanel layoutRadius;
        private GroupBox groupLog;
        private TableLayoutPanel layoutStatus;
        private Label labelStatus;
        private Label labelProgressPercent;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
