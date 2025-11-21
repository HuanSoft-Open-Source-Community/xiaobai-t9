
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
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(48, 32);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(65, 12);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "当前版本：";
            // 
            // chkAutoUpdateOnStartup
            // 
            this.chkAutoUpdateOnStartup.AutoSize = true;
            this.chkAutoUpdateOnStartup.Location = new System.Drawing.Point(50, 124);
            this.chkAutoUpdateOnStartup.Name = "chkAutoUpdateOnStartup";
            this.chkAutoUpdateOnStartup.Size = new System.Drawing.Size(132, 16);
            this.chkAutoUpdateOnStartup.TabIndex = 1;
            this.chkAutoUpdateOnStartup.Text = "每次开机时检查更新";
            this.chkAutoUpdateOnStartup.UseVisualStyleBackColor = true;
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.Location = new System.Drawing.Point(50, 74);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(132, 23);
            this.btnCheckUpdate.TabIndex = 2;
            this.btnCheckUpdate.Text = "检查更新";
            this.btnCheckUpdate.UseVisualStyleBackColor = true;
            this.btnCheckUpdate.Click += new System.EventHandler(this.btnCheckUpdate_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(97, 347);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(424, 21);
            this.textBox1.TabIndex = 3;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(97, 282);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(424, 21);
            this.textBox2.TabIndex = 4;
            // 
            // progressBarDownload
            // 
            this.progressBarDownload.Location = new System.Drawing.Point(50, 172);
            this.progressBarDownload.Name = "progressBarDownload";
            this.progressBarDownload.Size = new System.Drawing.Size(349, 23);
            this.progressBarDownload.TabIndex = 5;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(12, 390);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(445, 21);
            this.textBox3.TabIndex = 6;
            // 
            // btnForceDownload
            // 
            this.btnForceDownload.Location = new System.Drawing.Point(211, 74);
            this.btnForceDownload.Name = "btnForceDownload";
            this.btnForceDownload.Size = new System.Drawing.Size(132, 23);
            this.btnForceDownload.TabIndex = 7;
            this.btnForceDownload.Text = "强制下载最新版";
            this.btnForceDownload.UseVisualStyleBackColor = true;
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 234);
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
    }
}

