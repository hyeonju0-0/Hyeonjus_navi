namespace find_forestLoad
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox_x1 = new TextBox();
            textBox_y1 = new TextBox();
            textBox_x2 = new TextBox();
            textBox_y2 = new TextBox();
            textBox_z1 = new TextBox();
            textBox_z2 = new TextBox();
            textBox_slope = new TextBox();
            textBox_radius = new TextBox();
            textBox_result = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            labelLasFile = new Label();
            button1 = new Button();
            buttonOpenLas = new Button();
            buttonQueryZ = new Button();
            buttonPickStart = new Button();
            buttonPickEnd = new Button();
            buttonPreviewCad = new Button();
            progressBar1 = new ProgressBar();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel4 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // textBox_x1
            // 
            textBox_x1.Dock = DockStyle.Fill;
            textBox_x1.Font = new Font("맑은 고딕", 15F);
            textBox_x1.Location = new Point(216, 3);
            textBox_x1.Name = "textBox_x1";
            textBox_x1.Size = new Size(245, 34);
            textBox_x1.TabIndex = 0;
            // 
            // textBox_y1
            // 
            textBox_y1.Dock = DockStyle.Fill;
            textBox_y1.Font = new Font("맑은 고딕", 15F);
            textBox_y1.Location = new Point(216, 52);
            textBox_y1.Name = "textBox_y1";
            textBox_y1.Size = new Size(245, 34);
            textBox_y1.TabIndex = 1;
            // 
            // textBox_x2
            // 
            textBox_x2.Dock = DockStyle.Fill;
            textBox_x2.Font = new Font("맑은 고딕", 15F);
            textBox_x2.Location = new Point(216, 150);
            textBox_x2.Name = "textBox_x2";
            textBox_x2.Size = new Size(245, 34);
            textBox_x2.TabIndex = 3;
            // 
            // textBox_y2
            // 
            textBox_y2.Dock = DockStyle.Fill;
            textBox_y2.Font = new Font("맑은 고딕", 15F);
            textBox_y2.Location = new Point(216, 199);
            textBox_y2.Name = "textBox_y2";
            textBox_y2.Size = new Size(245, 34);
            textBox_y2.TabIndex = 4;
            // 
            // textBox_z1
            // 
            textBox_z1.Dock = DockStyle.Fill;
            textBox_z1.Font = new Font("맑은 고딕", 15F);
            textBox_z1.Location = new Point(216, 101);
            textBox_z1.Name = "textBox_z1";
            textBox_z1.ReadOnly = true;
            textBox_z1.Size = new Size(245, 34);
            textBox_z1.TabIndex = 2;
            textBox_z1.TabStop = false;
            // 
            // textBox_z2
            // 
            textBox_z2.Dock = DockStyle.Fill;
            textBox_z2.Font = new Font("맑은 고딕", 15F);
            textBox_z2.Location = new Point(216, 248);
            textBox_z2.Name = "textBox_z2";
            textBox_z2.ReadOnly = true;
            textBox_z2.Size = new Size(245, 34);
            textBox_z2.TabIndex = 5;
            textBox_z2.TabStop = false;
            // 
            // textBox_slope
            // 
            textBox_slope.Dock = DockStyle.Fill;
            textBox_slope.Font = new Font("맑은 고딕", 15F);
            textBox_slope.Location = new Point(216, 297);
            textBox_slope.Name = "textBox_slope";
            textBox_slope.Size = new Size(245, 34);
            textBox_slope.TabIndex = 6;
            // 
            // textBox_radius
            // 
            textBox_radius.Dock = DockStyle.Fill;
            textBox_radius.Font = new Font("맑은 고딕", 15F);
            textBox_radius.Location = new Point(216, 346);
            textBox_radius.Name = "textBox_radius";
            textBox_radius.Size = new Size(245, 34);
            textBox_radius.TabIndex = 7;
            textBox_radius.Text = "2";
            // 
            // textBox_result
            // 
            tableLayoutPanel1.SetColumnSpan(textBox_result, 3);
            textBox_result.Dock = DockStyle.Fill;
            textBox_result.Font = new Font("맑은 고딕", 10F);
            textBox_result.Location = new Point(12, 641);
            textBox_result.Margin = new Padding(12, 4, 12, 4);
            textBox_result.Multiline = true;
            textBox_result.Name = "textBox_result";
            textBox_result.ReadOnly = true;
            textBox_result.ScrollBars = ScrollBars.Vertical;
            textBox_result.Size = new Size(536, 129);
            textBox_result.TabIndex = 12;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("맑은 고딕", 14F);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(207, 49);
            label1.TabIndex = 0;
            label1.Text = "시점 x좌표";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("맑은 고딕", 14F);
            label2.Location = new Point(3, 49);
            label2.Name = "label2";
            label2.Size = new Size(207, 49);
            label2.TabIndex = 1;
            label2.Text = "시점 y좌표";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("맑은 고딕", 14F);
            label3.Location = new Point(3, 196);
            label3.Name = "label3";
            label3.Size = new Size(207, 49);
            label3.TabIndex = 4;
            label3.Text = "종점 y좌표";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Font = new Font("맑은 고딕", 14F);
            label4.Location = new Point(3, 147);
            label4.Name = "label4";
            label4.Size = new Size(207, 49);
            label4.TabIndex = 3;
            label4.Text = "종점 x좌표";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Fill;
            label5.Font = new Font("맑은 고딕", 14F);
            label5.Location = new Point(3, 294);
            label5.Name = "label5";
            label5.Size = new Size(207, 49);
            label5.TabIndex = 6;
            label5.Text = "허용 경사도";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Fill;
            label6.Font = new Font("맑은 고딕", 14F);
            label6.Location = new Point(3, 98);
            label6.Name = "label6";
            label6.Size = new Size(207, 49);
            label6.TabIndex = 2;
            label6.Text = "시점 고도";
            label6.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Dock = DockStyle.Fill;
            label7.Font = new Font("맑은 고딕", 14F);
            label7.Location = new Point(3, 245);
            label7.Name = "label7";
            label7.Size = new Size(207, 49);
            label7.TabIndex = 5;
            label7.Text = "종점 고도";
            label7.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Dock = DockStyle.Fill;
            label8.Font = new Font("맑은 고딕", 14F);
            label8.Location = new Point(3, 343);
            label8.Name = "label8";
            label8.Size = new Size(207, 55);
            label8.TabIndex = 7;
            label8.Text = "검색 반경(m)";
            label8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelLasFile
            // 
            labelLasFile.AutoEllipsis = true;
            labelLasFile.Dock = DockStyle.Fill;
            labelLasFile.Font = new Font("맑은 고딕", 10F);
            labelLasFile.Location = new Point(133, 0);
            labelLasFile.Name = "labelLasFile";
            labelLasFile.Padding = new Padding(8, 0, 0, 0);
            labelLasFile.Size = new Size(328, 58);
            labelLasFile.TabIndex = 1;
            labelLasFile.Text = "선택된 파일 없음";
            labelLasFile.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            button1.Dock = DockStyle.Fill;
            button1.Font = new Font("맑은 고딕", 13F);
            button1.Location = new Point(235, 3);
            button1.Name = "button1";
            button1.Size = new Size(226, 45);
            button1.TabIndex = 1;
            button1.Text = "임도 생성";
            button1.UseVisualStyleBackColor = true;
            // 
            // buttonOpenLas
            // 
            buttonOpenLas.Dock = DockStyle.Fill;
            buttonOpenLas.Font = new Font("맑은 고딕", 12F);
            buttonOpenLas.Location = new Point(3, 3);
            buttonOpenLas.Name = "buttonOpenLas";
            buttonOpenLas.Size = new Size(124, 52);
            buttonOpenLas.TabIndex = 0;
            buttonOpenLas.Text = "LAS 열기";
            buttonOpenLas.UseVisualStyleBackColor = true;
            // 
            // buttonQueryZ
            // 
            buttonQueryZ.Dock = DockStyle.Fill;
            buttonQueryZ.Font = new Font("맑은 고딕", 13F);
            buttonQueryZ.Location = new Point(3, 3);
            buttonQueryZ.Name = "buttonQueryZ";
            buttonQueryZ.Size = new Size(226, 45);
            buttonQueryZ.TabIndex = 0;
            buttonQueryZ.Text = "고도 조회";
            buttonQueryZ.UseVisualStyleBackColor = true;
            // 
            // buttonPickStart
            // 
            buttonPickStart.Dock = DockStyle.Fill;
            buttonPickStart.Font = new Font("맑은 고딕", 12F);
            buttonPickStart.Location = new Point(3, 54);
            buttonPickStart.Name = "buttonPickStart";
            buttonPickStart.Size = new Size(226, 45);
            buttonPickStart.TabIndex = 2;
            buttonPickStart.Text = "시점을 CAD에서";
            buttonPickStart.UseVisualStyleBackColor = true;
            // 
            // buttonPickEnd
            // 
            buttonPickEnd.Dock = DockStyle.Fill;
            buttonPickEnd.Font = new Font("맑은 고딕", 12F);
            buttonPickEnd.Location = new Point(235, 54);
            buttonPickEnd.Name = "buttonPickEnd";
            buttonPickEnd.Size = new Size(226, 45);
            buttonPickEnd.TabIndex = 3;
            buttonPickEnd.Text = "종점을 CAD에서";
            buttonPickEnd.UseVisualStyleBackColor = true;
            // 
            // buttonPreviewCad
            // 
            tableLayoutPanel3.SetColumnSpan(buttonPreviewCad, 2);
            buttonPreviewCad.Dock = DockStyle.Fill;
            buttonPreviewCad.Font = new Font("맑은 고딕", 12F);
            buttonPreviewCad.Location = new Point(3, 105);
            buttonPreviewCad.Name = "buttonPreviewCad";
            buttonPreviewCad.Size = new Size(458, 47);
            buttonPreviewCad.TabIndex = 4;
            buttonPreviewCad.Text = "후보 노선 선택 후 CAD에서 보기";
            buttonPreviewCad.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            tableLayoutPanel1.SetColumnSpan(progressBar1, 3);
            progressBar1.Dock = DockStyle.Fill;
            progressBar1.Location = new Point(12, 782);
            progressBar1.Margin = new Padding(12, 8, 12, 8);
            progressBar1.Maximum = 1000;
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(536, 26);
            progressBar1.TabIndex = 13;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 84F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 1, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 1, 2);
            tableLayoutPanel1.Controls.Add(textBox_result, 0, 3);
            tableLayoutPanel1.Controls.Add(progressBar1, 0, 4);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(0, 8, 0, 4);
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 8F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 17F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            tableLayoutPanel1.Size = new Size(560, 820);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(buttonOpenLas, 0, 0);
            tableLayoutPanel4.Controls.Add(labelLasFile, 1, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(47, 11);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(464, 58);
            tableLayoutPanel4.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Controls.Add(textBox_x1, 1, 0);
            tableLayoutPanel2.Controls.Add(label2, 0, 1);
            tableLayoutPanel2.Controls.Add(textBox_y1, 1, 1);
            tableLayoutPanel2.Controls.Add(label6, 0, 2);
            tableLayoutPanel2.Controls.Add(textBox_z1, 1, 2);
            tableLayoutPanel2.Controls.Add(label4, 0, 3);
            tableLayoutPanel2.Controls.Add(textBox_x2, 1, 3);
            tableLayoutPanel2.Controls.Add(label3, 0, 4);
            tableLayoutPanel2.Controls.Add(textBox_y2, 1, 4);
            tableLayoutPanel2.Controls.Add(label7, 0, 5);
            tableLayoutPanel2.Controls.Add(textBox_z2, 1, 5);
            tableLayoutPanel2.Controls.Add(label5, 0, 6);
            tableLayoutPanel2.Controls.Add(textBox_slope, 1, 6);
            tableLayoutPanel2.Controls.Add(label8, 0, 7);
            tableLayoutPanel2.Controls.Add(textBox_radius, 1, 7);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(47, 75);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 8;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel2.Size = new Size(464, 398);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(buttonQueryZ, 0, 0);
            tableLayoutPanel3.Controls.Add(button1, 1, 0);
            tableLayoutPanel3.Controls.Add(buttonPickStart, 0, 1);
            tableLayoutPanel3.Controls.Add(buttonPickEnd, 1, 1);
            tableLayoutPanel3.Controls.Add(buttonPreviewCad, 0, 2);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(47, 479);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
            tableLayoutPanel3.Size = new Size(464, 155);
            tableLayoutPanel3.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 820);
            Controls.Add(tableLayoutPanel1);
            MinimumSize = new Size(520, 680);
            Name = "Form1";
            Text = "현주의 길찾기";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox textBox_x1;
        private TextBox textBox_y1;
        private TextBox textBox_x2;
        private TextBox textBox_y2;
        private TextBox textBox_z1;
        private TextBox textBox_z2;
        private TextBox textBox_slope;
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
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
    }
}
