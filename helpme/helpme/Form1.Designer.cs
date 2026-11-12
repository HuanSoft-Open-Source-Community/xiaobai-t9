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
            this.lblStep1 = new System.Windows.Forms.Label();
            this.lblStep3 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lblShotHint = new System.Windows.Forms.Label();
            this.lblTip = new System.Windows.Forms.Label();
            this.adPanel = new System.Windows.Forms.Panel();
            this.btnToggleAd = new System.Windows.Forms.Button();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.BackColor = System.Drawing.Color.White;
            this.panelLeft.Controls.Add(this.txtLog);
            this.panelLeft.Controls.Add(this.lblLogHint);
            this.panelLeft.Controls.Add(this.lblStep3);
            this.panelLeft.Controls.Add(this.textBox1);
            this.panelLeft.Controls.Add(this.lblInputHint);
            this.panelLeft.Controls.Add(this.btnSwitch);
            this.panelLeft.Controls.Add(this.lblStep1);
            this.panelLeft.Controls.Add(this.lblTitle);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(580, 540);
            this.panelLeft.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 17F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lblTitle.Location = new System.Drawing.Point(34, 26);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(512, 40);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "欢迎使用小白T9输入法";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStep1
            // 
            this.lblStep1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblStep1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblStep1.Location = new System.Drawing.Point(34, 78);
            this.lblStep1.Name = "lblStep1";
            this.lblStep1.Size = new System.Drawing.Size(512, 26);
            this.lblStep1.TabIndex = 1;
            this.lblStep1.Text = "① 先切换到小白T9";
            // 
            // btnSwitch
            // 
            this.btnSwitch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSwitch.FlatAppearance.BorderSize = 0;
            this.btnSwitch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSwitch.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnSwitch.ForeColor = System.Drawing.Color.White;
            this.btnSwitch.Location = new System.Drawing.Point(34, 108);
            this.btnSwitch.Name = "btnSwitch";
            this.btnSwitch.Size = new System.Drawing.Size(512, 58);
            this.btnSwitch.TabIndex = 2;
            this.btnSwitch.Text = "一键切换到小白T9";
            this.btnSwitch.UseVisualStyleBackColor = false;
            this.btnSwitch.Click += new System.EventHandler(this.btnSwitch_Click_1);
            // 
            // lblInputHint
            // 
            this.lblInputHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblInputHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblInputHint.Location = new System.Drawing.Point(34, 182);
            this.lblInputHint.Name = "lblInputHint";
            this.lblInputHint.Size = new System.Drawing.Size(512, 26);
            this.lblInputHint.TabIndex = 3;
            this.lblInputHint.Text = "② 在这里随便打几个字试试";
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textBox1.Location = new System.Drawing.Point(34, 212);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(512, 34);
            this.textBox1.TabIndex = 4;
            // 
            // lblStep3
            // 
            this.lblStep3.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblStep3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblStep3.Location = new System.Drawing.Point(34, 258);
            this.lblStep3.Name = "lblStep3";
            this.lblStep3.Size = new System.Drawing.Size(512, 26);
            this.lblStep3.TabIndex = 5;
            this.lblStep3.Text = "③ 能正常打字就可以关掉这个窗口了";
            // 
            // lblLogHint
            // 
            this.lblLogHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblLogHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblLogHint.Location = new System.Drawing.Point(34, 300);
            this.lblLogHint.Name = "lblLogHint";
            this.lblLogHint.Size = new System.Drawing.Size(512, 22);
            this.lblLogHint.TabIndex = 6;
            this.lblLogHint.Text = "运行状态";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLog.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.txtLog.Location = new System.Drawing.Point(34, 326);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(512, 176);
            this.txtLog.TabIndex = 7;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(64, 64);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Visible = false;
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.panelRight.Controls.Add(this.lblTip);
            this.panelRight.Controls.Add(this.pictureBox1);
            this.panelRight.Controls.Add(this.lblShotHint);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(580, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(320, 540);
            this.panelRight.TabIndex = 1;
            // 
            // lblShotHint
            // 
            this.lblShotHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblShotHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lblShotHint.Location = new System.Drawing.Point(24, 30);
            this.lblShotHint.Name = "lblShotHint";
            this.lblShotHint.Size = new System.Drawing.Size(272, 26);
            this.lblShotHint.TabIndex = 1;
            this.lblShotHint.Text = "切换菜单长这样";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(24, 62);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(272, 236);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // lblTip
            // 
            this.lblTip.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblTip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblTip.Location = new System.Drawing.Point(24, 314);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(272, 120);
            this.lblTip.TabIndex = 3;
            this.lblTip.Text = "找不到也没关系：\r\n按 Win + 空格 也能切换输入法。\r\n\r\n切换后，用右边小键盘打字，\r\n像手机九键一样。";
            // 
            // adPanel
            // 
            this.adPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(250)))), ((int)(((byte)(240)))));
            this.adPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.adPanel.Location = new System.Drawing.Point(3000, 0);
            this.adPanel.Name = "adPanel";
            this.adPanel.Size = new System.Drawing.Size(190, 540);
            this.adPanel.TabIndex = 200;
            this.adPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.adPanel_Paint);
            // 
            // btnToggleAd
            // 
            this.btnToggleAd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.btnToggleAd.FlatAppearance.BorderSize = 0;
            this.btnToggleAd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleAd.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnToggleAd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.btnToggleAd.Location = new System.Drawing.Point(880, 240);
            this.btnToggleAd.Name = "btnToggleAd";
            this.btnToggleAd.Size = new System.Drawing.Size(20, 60);
            this.btnToggleAd.TabIndex = 201;
            this.btnToggleAd.Text = "<";
            this.btnToggleAd.UseVisualStyleBackColor = false;
            this.btnToggleAd.Click += new System.EventHandler(this.btnToggleAd_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 540);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.adPanel);
            this.Controls.Add(this.btnToggleAd);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "小白T9输入法";
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelRight.ResumeLayout(false);
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
        private System.Windows.Forms.Label lblStep1;
        private System.Windows.Forms.Label lblStep3;
        private System.Windows.Forms.Label lblShotHint;
        private System.Windows.Forms.Label lblTip;
        // 广告面板控件
        private System.Windows.Forms.Panel adPanel;
        private System.Windows.Forms.Button btnToggleAd;
    }
}
