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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            textBox_slope = new TextBox();
            button1 = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // textBox_x1
            // 
            textBox_x1.Dock = DockStyle.Fill;
            textBox_x1.Font = new Font("맑은 고딕", 15F);
            textBox_x1.Location = new Point(141, 3);
            textBox_x1.Name = "textBox_x1";
            textBox_x1.Size = new Size(132, 34);
            textBox_x1.TabIndex = 0;
            // 
            // textBox_y1
            // 
            textBox_y1.Dock = DockStyle.Fill;
            textBox_y1.Font = new Font("맑은 고딕", 15F);
            textBox_y1.Location = new Point(141, 64);
            textBox_y1.Name = "textBox_y1";
            textBox_y1.Size = new Size(132, 34);
            textBox_y1.TabIndex = 1;
            // 
            // textBox_x2
            // 
            textBox_x2.Dock = DockStyle.Fill;
            textBox_x2.Font = new Font("맑은 고딕", 15F);
            textBox_x2.Location = new Point(141, 125);
            textBox_x2.Name = "textBox_x2";
            textBox_x2.Size = new Size(132, 34);
            textBox_x2.TabIndex = 2;
            // 
            // textBox_y2
            // 
            textBox_y2.Dock = DockStyle.Fill;
            textBox_y2.Font = new Font("맑은 고딕", 15F);
            textBox_y2.Location = new Point(141, 186);
            textBox_y2.Name = "textBox_y2";
            textBox_y2.Size = new Size(132, 34);
            textBox_y2.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("맑은 고딕", 14F);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(132, 61);
            label1.TabIndex = 4;
            label1.Text = "시점1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("맑은 고딕", 14F);
            label2.Location = new Point(3, 61);
            label2.Name = "label2";
            label2.Size = new Size(132, 61);
            label2.TabIndex = 5;
            label2.Text = "시점2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("맑은 고딕", 14F);
            label3.Location = new Point(3, 183);
            label3.Name = "label3";
            label3.Size = new Size(132, 61);
            label3.TabIndex = 7;
            label3.Text = "종점2";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Font = new Font("맑은 고딕", 14F);
            label4.Location = new Point(3, 122);
            label4.Name = "label4";
            label4.Size = new Size(132, 61);
            label4.TabIndex = 6;
            label4.Text = "종점1";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Fill;
            label5.Font = new Font("맑은 고딕", 14F);
            label5.Location = new Point(3, 244);
            label5.Name = "label5";
            label5.Size = new Size(132, 65);
            label5.TabIndex = 8;
            label5.Text = "허용 경사도";
            // 
            // textBox_slope
            // 
            textBox_slope.Dock = DockStyle.Fill;
            textBox_slope.Font = new Font("맑은 고딕", 15F);
            textBox_slope.Location = new Point(141, 247);
            textBox_slope.Name = "textBox_slope";
            textBox_slope.Size = new Size(132, 34);
            textBox_slope.TabIndex = 9;
            // 
            // button1
            // 
            button1.Dock = DockStyle.Fill;
            button1.Font = new Font("맑은 고딕", 13F);
            button1.Location = new Point(58, 3);
            button1.Name = "button1";
            button1.Size = new Size(159, 55);
            button1.TabIndex = 10;
            button1.Text = "임도 생성";
            button1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            tableLayoutPanel1.Size = new Size(353, 450);
            tableLayoutPanel1.TabIndex = 11;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Controls.Add(label2, 0, 1);
            tableLayoutPanel2.Controls.Add(label4, 0, 2);
            tableLayoutPanel2.Controls.Add(textBox_slope, 1, 4);
            tableLayoutPanel2.Controls.Add(label3, 0, 3);
            tableLayoutPanel2.Controls.Add(textBox_y2, 1, 3);
            tableLayoutPanel2.Controls.Add(label5, 0, 4);
            tableLayoutPanel2.Controls.Add(textBox_x2, 1, 2);
            tableLayoutPanel2.Controls.Add(textBox_x1, 1, 0);
            tableLayoutPanel2.Controls.Add(textBox_y1, 1, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(38, 48);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 5;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.Size = new Size(276, 309);
            tableLayoutPanel2.TabIndex = 12;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.Controls.Add(button1, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(38, 363);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(276, 61);
            tableLayoutPanel3.TabIndex = 13;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(353, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "Form1";
            Text = "현주의 길찾기";
            tableLayoutPanel1.ResumeLayout(false);
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
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox textBox_slope;
        private Button button1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
    }
}
