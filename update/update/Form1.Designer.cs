
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.progressBarDownload = new System.Windows.Forms.ProgressBar();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.btnForceDownload = new System.Windows.Forms.Button();
            this.lblLatestVersion = new System.Windows.Forms.Label();
            this.btnManageGram = new System.Windows.Forms.Button();
            this.lblGramStatus2 = new System.Windows.Forms.Label();
            this.lblGramStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(48, 31);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(65, 12);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "当前版本：";
            // 
            // chkAutoUpdateOnStartup
            // 
            this.chkAutoUpdateOnStartup.AutoSize = true;
            this.chkAutoUpdateOnStartup.Location = new System.Drawing.Point(50, 138);
            this.chkAutoUpdateOnStartup.Name = "chkAutoUpdateOnStartup";
            this.chkAutoUpdateOnStartup.Size = new System.Drawing.Size(132, 16);
            this.chkAutoUpdateOnStartup.TabIndex = 1;
            this.chkAutoUpdateOnStartup.Text = "每次开机时检查更新";
            this.chkAutoUpdateOnStartup.UseVisualStyleBackColor = true;
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.Location = new System.Drawing.Point(50, 96);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(319, 23);
            this.btnCheckUpdate.TabIndex = 2;
            this.btnCheckUpdate.Text = "检查更新";
            this.btnCheckUpdate.UseVisualStyleBackColor = true;
            this.btnCheckUpdate.Click += new System.EventHandler(this.btnCheckUpdate_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(103, 519);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(424, 21);
            this.textBox1.TabIndex = 3;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(103, 454);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(424, 21);
            this.textBox2.TabIndex = 4;
            // 
            // progressBarDownload
            // 
            this.progressBarDownload.Location = new System.Drawing.Point(50, 288);
            this.progressBarDownload.Name = "progressBarDownload";
            this.progressBarDownload.Size = new System.Drawing.Size(450, 23);
            this.progressBarDownload.TabIndex = 5;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(18, 562);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(445, 21);
            this.textBox3.TabIndex = 6;
            // 
            // btnForceDownload
            // 
            this.btnForceDownload.Location = new System.Drawing.Point(50, 207);
            this.btnForceDownload.Name = "btnForceDownload";
            this.btnForceDownload.Size = new System.Drawing.Size(143, 23);
            this.btnForceDownload.TabIndex = 7;
            this.btnForceDownload.Text = "打开官网下载页面";
            this.btnForceDownload.UseVisualStyleBackColor = true;
            this.btnForceDownload.Click += new System.EventHandler(this.btnForceDownload_Click_1);
            // 
            // lblLatestVersion
            // 
            this.lblLatestVersion.AutoSize = true;
            this.lblLatestVersion.Location = new System.Drawing.Point(48, 61);
            this.lblLatestVersion.Name = "lblLatestVersion";
            this.lblLatestVersion.Size = new System.Drawing.Size(65, 12);
            this.lblLatestVersion.TabIndex = 8;
            this.lblLatestVersion.Text = "最新版本：";
            // 
            // btnManageGram
            // 
            this.btnManageGram.Location = new System.Drawing.Point(167, 406);
            this.btnManageGram.Name = "btnManageGram";
            this.btnManageGram.Size = new System.Drawing.Size(333, 23);
            this.btnManageGram.TabIndex = 9;
            this.btnManageGram.Text = "下载万象语法模型";
            this.btnManageGram.UseVisualStyleBackColor = true;
            // 
            // lblGramStatus2
            // 
            this.lblGramStatus2.AutoSize = true;
            this.lblGramStatus2.Location = new System.Drawing.Point(48, 411);
            this.lblGramStatus2.Name = "lblGramStatus2";
            this.lblGramStatus2.Size = new System.Drawing.Size(113, 12);
            this.lblGramStatus2.TabIndex = 10;
            this.lblGramStatus2.Text = "万象语法模型管理：";
            // 
            // lblGramStatus
            // 
            this.lblGramStatus.AutoSize = true;
            this.lblGramStatus.Location = new System.Drawing.Point(39, 375);
            this.lblGramStatus.Name = "lblGramStatus";
            this.lblGramStatus.Size = new System.Drawing.Size(143, 12);
            this.lblGramStatus.TabIndex = 11;
            this.lblGramStatus.Text = "万象语法模型: 检测中...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label1.Location = new System.Drawing.Point(50, 331);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(449, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "请注意，安装万象大模型后会导致每次重新部署需要一分钟生效时间，请酌情安装。";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(226, 207);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(143, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "直接开始下载";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(201, 212);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "或";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label3.Location = new System.Drawing.Point(106, 246);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(209, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "请注意，官网下载链接在置顶帖最下方";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label4.Location = new System.Drawing.Point(48, 176);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(323, 12);
            this.label4.TabIndex = 16;
            this.label4.Text = "-----------------------------------------------------";
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 271);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblGramStatus);
            this.Controls.Add(this.lblGramStatus2);
            this.Controls.Add(this.btnManageGram);
            this.Controls.Add(this.lblLatestVersion);
            this.Controls.Add(this.btnForceDownload);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.progressBarDownload);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnCheckUpdate);
            this.Controls.Add(this.chkAutoUpdateOnStartup);
            this.Controls.Add(this.lblVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "小白T9更新程序";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.CheckBox chkAutoUpdateOnStartup;
        private System.Windows.Forms.Button btnCheckUpdate;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ProgressBar progressBarDownload;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button btnForceDownload;
        private System.Windows.Forms.Label lblLatestVersion;
        private System.Windows.Forms.Button btnManageGram;
        private System.Windows.Forms.Label lblGramStatus2;
        private System.Windows.Forms.Label lblGramStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}

