namespace find_forestLoad
{
    partial class CadRoutePreviewDialog
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
            buttonSafety = new Button();
            buttonCost = new Button();
            buttonDistance = new Button();
            buttonTime = new Button();
            buttonShowAll = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // buttonSafety
            // 
            buttonSafety.BackColor = Color.FromArgb(244, 220, 213);
            buttonSafety.Dock = DockStyle.Fill;
            buttonSafety.Location = new Point(46, 23);
            buttonSafety.Margin = new Padding(3, 3, 3, 8);
            buttonSafety.Name = "buttonSafety";
            buttonSafety.Size = new Size(341, 51);
            buttonSafety.TabIndex = 0;
            buttonSafety.Text = "안정성  ·  꼭짓점 0개";
            buttonSafety.UseVisualStyleBackColor = false;
            buttonSafety.Click += buttonSafety_Click;
            // 
            // buttonCost
            // 
            buttonCost.BackColor = Color.FromArgb(244, 220, 213);
            buttonCost.Dock = DockStyle.Fill;
            buttonCost.Location = new Point(46, 85);
            buttonCost.Margin = new Padding(3, 3, 3, 8);
            buttonCost.Name = "buttonCost";
            buttonCost.Size = new Size(341, 51);
            buttonCost.TabIndex = 1;
            buttonCost.Text = "비용 추정  ·  꼭짓점 0개";
            buttonCost.UseVisualStyleBackColor = false;
            buttonCost.Click += buttonCost_Click;
            // 
            // buttonDistance
            // 
            buttonDistance.BackColor = Color.FromArgb(244, 220, 213);
            buttonDistance.Dock = DockStyle.Fill;
            buttonDistance.Location = new Point(46, 147);
            buttonDistance.Margin = new Padding(3, 3, 3, 8);
            buttonDistance.Name = "buttonDistance";
            buttonDistance.Size = new Size(341, 51);
            buttonDistance.TabIndex = 2;
            buttonDistance.Text = "거리 후보 / 부드러운 우회 초안  ·  꼭짓점 0개";
            buttonDistance.UseVisualStyleBackColor = false;
            buttonDistance.Click += buttonDistance_Click;
            // 
            // buttonTime
            // 
            buttonTime.BackColor = Color.FromArgb(244, 220, 213);
            buttonTime.Dock = DockStyle.Fill;
            buttonTime.Location = new Point(46, 209);
            buttonTime.Margin = new Padding(3, 3, 3, 8);
            buttonTime.Name = "buttonTime";
            buttonTime.Size = new Size(341, 51);
            buttonTime.TabIndex = 3;
            buttonTime.Text = "공사 기간 추정  ·  꼭짓점 0개";
            buttonTime.UseVisualStyleBackColor = false;
            buttonTime.Click += buttonTime_Click;
            // 
            // buttonShowAll
            // 
            buttonShowAll.BackColor = Color.FromArgb(244, 220, 213);
            buttonShowAll.Dock = DockStyle.Fill;
            buttonShowAll.Location = new Point(46, 275);
            buttonShowAll.Margin = new Padding(3, 7, 3, 8);
            buttonShowAll.Name = "buttonShowAll";
            buttonShowAll.Size = new Size(341, 47);
            buttonShowAll.TabIndex = 4;
            buttonShowAll.Text = "모든 노선 보기";
            buttonShowAll.UseVisualStyleBackColor = false;
            buttonShowAll.Click += buttonShowAll_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.Controls.Add(buttonShowAll, 1, 5);
            tableLayoutPanel1.Controls.Add(buttonTime, 1, 4);
            tableLayoutPanel1.Controls.Add(buttonDistance, 1, 3);
            tableLayoutPanel1.Controls.Add(buttonCost, 1, 2);
            tableLayoutPanel1.Controls.Add(buttonSafety, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(434, 351);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // CadRoutePreviewDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(434, 351);
            Controls.Add(tableLayoutPanel1);
            Font = new Font("맑은 고딕", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CadRoutePreviewDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "CAD에 표시할 후보 노선 선택";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button buttonSafety;
        private Button buttonCost;
        private Button buttonDistance;
        private Button buttonTime;
        private Button buttonShowAll;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
