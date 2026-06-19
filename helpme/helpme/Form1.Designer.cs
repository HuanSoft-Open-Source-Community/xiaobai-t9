namespace helpme
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnSwitch = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInputHint = new System.Windows.Forms.Label();
            this.lblLogHint = new System.Windows.Forms.Label();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelRight = new System.Windows.Forms.Panel();
            this.adPanel = new System.Windows.Forms.Panel();
            this.btnToggleAd = new System.Windows.Forms.Button();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSwitch
            // 
            this.btnSwitch.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSwitch.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.btnSwitch.ForeColor = System.Drawing.Color.White;
            this.btnSwitch.Location = new System.Drawing.Point(25, 25);
            this.btnSwitch.Name = "btnSwitch";
            this.btnSwitch.Size = new System.Drawing.Size(330, 62);
            this.btnSwitch.TabIndex = 1;
            this.btnSwitch.Text = "▶ 点我试试。。";
            this.btnSwitch.UseVisualStyleBackColor = false;
            this.btnSwitch.Click += new System.EventHandler(this.btnSwitch_Click_1);
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox1.Font = new System.Drawing.Font("Consolas", 16F);
            this.textBox1.Location = new System.Drawing.Point(25, 291);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(330, 32);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(100)))));
            this.txtLog.Location = new System.Drawing.Point(20, 60);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(503, 268);
            this.txtLog.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblTitle.Location = new System.Drawing.Point(25, 87);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(330, 60);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInputHint
            // 
            this.lblInputHint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblInputHint.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblInputHint.ForeColor = System.Drawing.Color.Gray;
            this.lblInputHint.Location = new System.Drawing.Point(25, 251);
            this.lblInputHint.Name = "lblInputHint";
            this.lblInputHint.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblInputHint.Size = new System.Drawing.Size(330, 40);
            this.lblInputHint.TabIndex = 2;
            this.lblInputHint.Text = "↓ 切换完成后，将在此处测试打字：";
            this.lblInputHint.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblLogHint
            // 
            this.lblLogHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogHint.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.lblLogHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblLogHint.Location = new System.Drawing.Point(20, 20);
            this.lblLogHint.Name = "lblLogHint";
            this.lblLogHint.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblLogHint.Size = new System.Drawing.Size(503, 40);
            this.lblLogHint.TabIndex = 1;
            this.lblLogHint.Text = "[ 实时运行日志 ]";
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.pictureBox2);
            this.panelLeft.Controls.Add(this.pictureBox1);
            this.panelLeft.Controls.Add(this.lblTitle);
            this.panelLeft.Controls.Add(this.btnSwitch);
            this.panelLeft.Controls.Add(this.lblInputHint);
            this.panelLeft.Controls.Add(this.textBox1);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(25);
            this.panelLeft.Size = new System.Drawing.Size(380, 348);
            this.panelLeft.TabIndex = 1;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(25, 94);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(95, 164);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(126, 93);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(229, 178);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.panelRight.Controls.Add(this.txtLog);
            this.panelRight.Controls.Add(this.lblLogHint);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(380, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(20);
            this.panelRight.Size = new System.Drawing.Size(543, 348);
            this.panelRight.TabIndex = 0;
            // 
            // adPanel
            // 
            this.adPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.adPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.adPanel.Location = new System.Drawing.Point(3000, 0);
            this.adPanel.Name = "adPanel";
            this.adPanel.Size = new System.Drawing.Size(190, 350);
            this.adPanel.TabIndex = 200;
            this.adPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.adPanel_Paint);
            // 
            // btnToggleAd
            // 
            this.btnToggleAd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(230)))), ((int)(((byte)(200)))));
            this.btnToggleAd.FlatAppearance.BorderSize = 0;
            this.btnToggleAd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleAd.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnToggleAd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(100)))), ((int)(((byte)(50)))));
            this.btnToggleAd.Location = new System.Drawing.Point(827, 145);
            this.btnToggleAd.Name = "btnToggleAd";
            this.btnToggleAd.Size = new System.Drawing.Size(20, 60);
            this.btnToggleAd.TabIndex = 201;
            this.btnToggleAd.Text = "<";
            this.btnToggleAd.UseVisualStyleBackColor = false;
            this.btnToggleAd.Click += new System.EventHandler(this.btnToggleAd_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(923, 348);
            this.Controls.Add(this.adPanel);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.btnToggleAd);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "小白T9输入法 HelpMe v2.1";
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSwitch;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInputHint;
        private System.Windows.Forms.Label lblLogHint;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        // 广告面板控件
        private System.Windows.Forms.Panel adPanel;
        private System.Windows.Forms.Button btnToggleAd;
    }
}
