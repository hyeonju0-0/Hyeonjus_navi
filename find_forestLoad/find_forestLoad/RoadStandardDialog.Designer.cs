namespace find_forestLoad
{
    partial class RoadStandardDialog
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
            tableLayoutPanel = new TableLayoutPanel();
            labelRoadKind = new Label();
            roadKind = new ComboBox();
            labelTerrainKind = new Label();
            terrainKind = new ComboBox();
            labelDesignSpeed = new Label();
            designSpeed = new ComboBox();
            labelMaximumGrade = new Label();
            gradeLayout = new TableLayoutPanel();
            maximumGrade = new NumericUpDown();
            resetGradeButton = new Button();
            summary = new Label();
            confirmButton = new Button();
            tableLayoutPanel.SuspendLayout();
            gradeLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)maximumGrade).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(labelRoadKind, 0, 0);
            tableLayoutPanel.Controls.Add(roadKind, 1, 0);
            tableLayoutPanel.Controls.Add(labelTerrainKind, 0, 1);
            tableLayoutPanel.Controls.Add(terrainKind, 1, 1);
            tableLayoutPanel.Controls.Add(labelDesignSpeed, 0, 2);
            tableLayoutPanel.Controls.Add(designSpeed, 1, 2);
            tableLayoutPanel.Controls.Add(labelMaximumGrade, 0, 3);
            tableLayoutPanel.Controls.Add(gradeLayout, 1, 3);
            tableLayoutPanel.Controls.Add(summary, 0, 4);
            tableLayoutPanel.Controls.Add(confirmButton, 1, 5);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.Padding = new Padding(16);
            tableLayoutPanel.RowCount = 6;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.Size = new Size(560, 400);
            tableLayoutPanel.TabIndex = 0;
            // 
            // labelRoadKind
            // 
            labelRoadKind.Dock = DockStyle.Fill;
            labelRoadKind.Location = new Point(19, 16);
            labelRoadKind.Name = "labelRoadKind";
            labelRoadKind.Size = new Size(124, 45);
            labelRoadKind.TabIndex = 0;
            labelRoadKind.Text = "임도 종류";
            labelRoadKind.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // roadKind
            // 
            roadKind.Dock = DockStyle.Fill;
            roadKind.DropDownStyle = ComboBoxStyle.DropDownList;
            roadKind.FormattingEnabled = true;
            roadKind.Items.AddRange(new object[] { "간선임도", "산불진화임도", "작업임도" });
            roadKind.Location = new Point(149, 19);
            roadKind.Name = "roadKind";
            roadKind.Size = new Size(392, 23);
            roadKind.TabIndex = 1;
            roadKind.SelectedIndex = 0;
            // 
            // labelTerrainKind
            // 
            labelTerrainKind.Dock = DockStyle.Fill;
            labelTerrainKind.Location = new Point(19, 61);
            labelTerrainKind.Name = "labelTerrainKind";
            labelTerrainKind.Size = new Size(124, 45);
            labelTerrainKind.TabIndex = 2;
            labelTerrainKind.Text = "지형";
            labelTerrainKind.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // terrainKind
            // 
            terrainKind.Dock = DockStyle.Fill;
            terrainKind.DropDownStyle = ComboBoxStyle.DropDownList;
            terrainKind.FormattingEnabled = true;
            terrainKind.Items.AddRange(new object[] { "일반지형", "특수지형" });
            terrainKind.Location = new Point(149, 64);
            terrainKind.Name = "terrainKind";
            terrainKind.Size = new Size(392, 23);
            terrainKind.TabIndex = 3;
            terrainKind.SelectedIndex = 0;
            // 
            // labelDesignSpeed
            // 
            labelDesignSpeed.Dock = DockStyle.Fill;
            labelDesignSpeed.Location = new Point(19, 106);
            labelDesignSpeed.Name = "labelDesignSpeed";
            labelDesignSpeed.Size = new Size(124, 45);
            labelDesignSpeed.TabIndex = 4;
            labelDesignSpeed.Text = "설계속도";
            labelDesignSpeed.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // designSpeed
            // 
            designSpeed.Dock = DockStyle.Fill;
            designSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            designSpeed.FormattingEnabled = true;
            designSpeed.Items.AddRange(new object[] { "20", "30", "40" });
            designSpeed.Location = new Point(149, 109);
            designSpeed.Name = "designSpeed";
            designSpeed.Size = new Size(392, 23);
            designSpeed.TabIndex = 5;
            designSpeed.SelectedIndex = 0;
            // 
            // labelMaximumGrade
            // 
            labelMaximumGrade.Dock = DockStyle.Fill;
            labelMaximumGrade.Location = new Point(19, 151);
            labelMaximumGrade.Name = "labelMaximumGrade";
            labelMaximumGrade.Size = new Size(124, 45);
            labelMaximumGrade.TabIndex = 6;
            labelMaximumGrade.Text = "허용 경사(%)";
            labelMaximumGrade.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gradeLayout
            // 
            gradeLayout.ColumnCount = 2;
            gradeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            gradeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            gradeLayout.Controls.Add(maximumGrade, 0, 0);
            gradeLayout.Controls.Add(resetGradeButton, 1, 0);
            gradeLayout.Dock = DockStyle.Fill;
            gradeLayout.Location = new Point(149, 151);
            gradeLayout.Margin = new Padding(0);
            gradeLayout.Name = "gradeLayout";
            gradeLayout.RowCount = 1;
            gradeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gradeLayout.Size = new Size(395, 45);
            gradeLayout.TabIndex = 7;
            // 
            // maximumGrade
            // 
            maximumGrade.DecimalPlaces = 1;
            maximumGrade.Dock = DockStyle.Fill;
            maximumGrade.Increment = 0.5M;
            maximumGrade.Location = new Point(3, 3);
            maximumGrade.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            maximumGrade.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            maximumGrade.Name = "maximumGrade";
            maximumGrade.Size = new Size(293, 23);
            maximumGrade.TabIndex = 0;
            maximumGrade.TextAlign = HorizontalAlignment.Right;
            maximumGrade.ThousandsSeparator = false;
            maximumGrade.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // resetGradeButton
            // 
            resetGradeButton.Dock = DockStyle.Fill;
            resetGradeButton.Location = new Point(307, 3);
            resetGradeButton.Margin = new Padding(8, 0, 0, 0);
            resetGradeButton.Name = "resetGradeButton";
            resetGradeButton.Size = new Size(88, 39);
            resetGradeButton.TabIndex = 1;
            resetGradeButton.Text = "기준값";
            resetGradeButton.UseVisualStyleBackColor = true;
            // 
            // summary
            // 
            summary.AutoSize = false;
            tableLayoutPanel.SetColumnSpan(summary, 2);
            summary.Dock = DockStyle.Fill;
            summary.Location = new Point(19, 196);
            summary.Name = "summary";
            summary.Size = new Size(522, 143);
            summary.TabIndex = 8;
            summary.Text = "적용 종단경사 한도: 9% (기준표)\r\n유효너비 기준: 3m\r\n배향곡선지 너비: 6m 이상\r\n\r\n경사 칸에 값을 넣으면 그 한도로 탐색합니다. 기준값은 표 값으로 되돌립니다.\r\n현재 A*는 경사만 탐색에 반영합니다.\r\n도로 폭·곡선·배수는 다음 단계에서 검토합니다.";
            // 
            // confirmButton
            // 
            confirmButton.DialogResult = DialogResult.OK;
            confirmButton.Dock = DockStyle.Right;
            confirmButton.Location = new Point(386, 342);
            confirmButton.Name = "confirmButton";
            confirmButton.Size = new Size(155, 39);
            confirmButton.TabIndex = 9;
            confirmButton.Text = "이 기준으로 탐색";
            confirmButton.UseVisualStyleBackColor = true;
            // 
            // RoadStandardDialog
            // 
            AcceptButton = confirmButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 400);
            Controls.Add(tableLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RoadStandardDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "임도 종류와 적용 기준";
            tableLayoutPanel.ResumeLayout(false);
            gradeLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)maximumGrade).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel;
        private Label labelRoadKind;
        private ComboBox roadKind;
        private Label labelTerrainKind;
        private ComboBox terrainKind;
        private Label labelDesignSpeed;
        private ComboBox designSpeed;
        private Label labelMaximumGrade;
        private TableLayoutPanel gradeLayout;
        private NumericUpDown maximumGrade;
        private Button resetGradeButton;
        private Label summary;
        private Button confirmButton;
    }
}
