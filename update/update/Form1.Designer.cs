
namespace XiaobaiT9Updater
{
    partial class UpdateForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateForm));
            this.lblVersion = new System.Windows.Forms.Label();
            this.chkAutoUpdateOnStartup = new System.Windows.Forms.CheckBox();
            this.btnCheckUpdate = new System.Windows.Forms.Button();
            this.progressBarDownload = new System.Windows.Forms.ProgressBar();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.btnForceDownload = new System.Windows.Forms.Button();
            this.lblLatestVersion = new System.Windows.Forms.Label();
            this.btnManageGram = new System.Windows.Forms.Button();
            this.lblGramStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(30, 28);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(65, 17);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "当前版本：";
            // 
            // lblLatestVersion
            // 
            this.lblLatestVersion.AutoSize = true;
            this.lblLatestVersion.Location = new System.Drawing.Point(30, 54);
            this.lblLatestVersion.Name = "lblLatestVersion";
            this.lblLatestVersion.Size = new System.Drawing.Size(65, 17);
            this.lblLatestVersion.TabIndex = 8;
            this.lblLatestVersion.Text = "最新版本：";
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnCheckUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCheckUpdate.FlatAppearance.BorderSize = 0;
            this.btnCheckUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCheckUpdate.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCheckUpdate.ForeColor = System.Drawing.Color.White;
            this.btnCheckUpdate.Location = new System.Drawing.Point(30, 84);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(380, 34);
            this.btnCheckUpdate.TabIndex = 2;
            this.btnCheckUpdate.Text = "检查更新";
            this.btnCheckUpdate.UseVisualStyleBackColor = false;
            this.btnCheckUpdate.Click += new System.EventHandler(this.btnCheckUpdate_Click);
            // 
            // chkAutoUpdateOnStartup
            // 
            this.chkAutoUpdateOnStartup.AutoSize = true;
            this.chkAutoUpdateOnStartup.Location = new System.Drawing.Point(30, 130);
            this.chkAutoUpdateOnStartup.Name = "chkAutoUpdateOnStartup";
            this.chkAutoUpdateOnStartup.Size = new System.Drawing.Size(132, 21);
            this.chkAutoUpdateOnStartup.TabIndex = 1;
            this.chkAutoUpdateOnStartup.Text = "每次开机时检查更新";
            this.chkAutoUpdateOnStartup.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = false;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(231)))), ((int)(((byte)(236)))));
            this.label4.Location = new System.Drawing.Point(30, 166);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(380, 1);
            this.label4.TabIndex = 16;
            this.label4.Text = "";
            // 
            // btnForceDownload
            // 
            this.btnForceDownload.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnForceDownload.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(213)))), ((int)(((byte)(221)))));
            this.btnForceDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForceDownload.Location = new System.Drawing.Point(30, 186);
            this.btnForceDownload.Name = "btnForceDownload";
            this.btnForceDownload.Size = new System.Drawing.Size(180, 32);
            this.btnForceDownload.TabIndex = 7;
            this.btnForceDownload.Text = "打开官网下载页面";
            this.btnForceDownload.UseVisualStyleBackColor = false;
            this.btnForceDownload.Click += new System.EventHandler(this.btnForceDownload_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.label2.Location = new System.Drawing.Point(214, 194);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "或";
            // 
            // button1
            // 
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(213)))), ((int)(((byte)(221)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(237, 186);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(173, 32);
            this.button1.TabIndex = 13;
            this.button1.Text = "直接开始下载";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.label3.Location = new System.Drawing.Point(30, 228);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(380, 17);
            this.label3.TabIndex = 15;
            this.label3.Text = "官网下载链接在置顶帖最下方";
            // 
            // label5
            // 
            this.label5.AutoSize = false;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(231)))), ((int)(((byte)(236)))));
            this.label5.Location = new System.Drawing.Point(30, 258);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(380, 1);
            this.label5.TabIndex = 17;
            this.label5.Text = "";
            // 
            // lblGramStatus
            // 
            this.lblGramStatus.AutoSize = true;
            this.lblGramStatus.Location = new System.Drawing.Point(30, 274);
            this.lblGramStatus.Name = "lblGramStatus";
            this.lblGramStatus.Size = new System.Drawing.Size(143, 17);
            this.lblGramStatus.TabIndex = 11;
            this.lblGramStatus.Text = "万象语法模型: 检测中...";
            // 
            // btnManageGram
            // 
            this.btnManageGram.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageGram.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(213)))), ((int)(((byte)(221)))));
            this.btnManageGram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageGram.Location = new System.Drawing.Point(30, 300);
            this.btnManageGram.Name = "btnManageGram";
            this.btnManageGram.Size = new System.Drawing.Size(380, 32);
            this.btnManageGram.TabIndex = 9;
            this.btnManageGram.Text = "下载万象语法模型（约 398 MB）";
            this.btnManageGram.UseVisualStyleBackColor = false;
            // 
            // progressBarDownload
            // 
            this.progressBarDownload.Location = new System.Drawing.Point(30, 340);
            this.progressBarDownload.Name = "progressBarDownload";
            this.progressBarDownload.Size = new System.Drawing.Size(380, 10);
            this.progressBarDownload.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.label1.Location = new System.Drawing.Point(30, 356);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(380, 46);
            this.label1.TabIndex = 12;
            this.label1.Text = "启用后每次重新部署约需一分钟，请酌情安装。\r\n需要引擎包含 Octagram 组件，否则不生效。";
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox3.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.textBox3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.textBox3.Location = new System.Drawing.Point(30, 412);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(380, 176);
            this.textBox3.TabIndex = 6;
            this.textBox3.WordWrap = false;
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(440, 618);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblGramStatus);
            this.Controls.Add(this.btnManageGram);
            this.Controls.Add(this.lblLatestVersion);
            this.Controls.Add(this.btnForceDownload);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.progressBarDownload);
            this.Controls.Add(this.btnCheckUpdate);
            this.Controls.Add(this.chkAutoUpdateOnStartup);
            this.Controls.Add(this.lblVersion);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "小白T9 · 检查更新";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.CheckBox chkAutoUpdateOnStartup;
        private System.Windows.Forms.Button btnCheckUpdate;
        private System.Windows.Forms.ProgressBar progressBarDownload;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button btnForceDownload;
        private System.Windows.Forms.Label lblLatestVersion;
        private System.Windows.Forms.Button btnManageGram;
        private System.Windows.Forms.Label lblGramStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}
