using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using IWshRuntimeLibrary;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace t9configui
{
    public partial class config : Form
    {
        public config()
        {
            InitializeComponent();
        }

        private bool _adCollapsed = false;
        private readonly string _adStateFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "t9config_ad_state.txt");

        private static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// 统一外观：浅灰底 + 白底细边按钮，去掉 WinForms 默认的立体感。
        /// 只改配色与按钮样式，不动任何控件位置和尺寸，避免影响既有布局。
        /// </summary>

        // ===== 模糊音折叠：这组设置是高级项，默认收起，避免把设置页撑得又长又乱 =====





        private void ApplyModernTheme()
        {
            this.BackColor = Color.FromArgb(242, 243, 245);
            StyleButtons(this);
            UnifyFonts(this);
            RebuildLayout();
        }

        // ==================== 界面重写：表格驱动，不再依赖设计器坐标 ====================

        private const int CardX = 16;
        private const int CardW = 612;
        private const int PadX = 16;
        private const int LeftValX = 104;      // 左列控件相对卡片
        private const int RightLabelX = 292;   // 右列标签相对卡片
        private const int RightValX = 388;     // 右列控件相对卡片
        private const int RowH = 32;
        private const int BtnW = 66;
        private const int BtnH = 27;
        private const int Gap = 6;

        private Panel _cardBasic, _cardFuzzy, _cardMisc;
        private Label _titleBasic, _titleMisc;
        private Button _fuzzyToggle;
        private Label _hint;
        private CheckBox _chkPredict;
        private ToolTip _tip;
        private bool _fuzzyShown;

        private void RebuildLayout()
        {
            SuspendLayout();

            HideInternal(快捷方式文件指向的路径);
            HideInternal(快捷方式文件指向的目标目录);
            HideInternal(label19);
            HideInternal(label5);      // 设计器里的「模糊音：」，改用卡片标题
            HideInternal(label29);     // 老提示，统一用新的

            if (_chkPredict == null) BuildPredictSwitch();

            int y = 10;

            _titleBasic = EnsureTitle(_titleBasic, "基础设置", ref y);
            _cardBasic = EnsureCard(_cardBasic, y);
            var basicRows = new System.Collections.Generic.List<object[]>
            {
                new object[] { "编码提示：",   new Control[] { bmts1, bmts2 } },
                new object[] { "候选大小：",   new Control[] { hxdx1, hxdx2 } },
                new object[] { "7 键功能：",   new Control[] { qjgn1, qjgn2 } },
                new object[] { "颜文字：",     new Control[] { ywz1, ywz2 } },
                new object[] { "多一码候选：", new Control[] { dymhx1, dymhx2 } },
                new object[] { "编码展示：",   new Control[] { bmzs1, bmzs2 } },
                new object[] { "候选方向：",   new Control[] { hxfx1, hxfx2 } },
                new object[] { "候选数量：",   new Control[] { hxsl1, hxsl2 } },
                new object[] { "方向键功能：", new Control[] { fxjgn1, fxjgn2 } },
                new object[] { "emoji：",      new Control[] { emoji1, emoji2 } },
                new object[] { "超四字联想：", new Control[] { cszlx1, cszlx2 } },
                new object[] { "联想提示：",   new Control[] { _chkPredict } },
            };
            _cardBasic.Height = LayoutRows(_cardBasic, basicRows);
            y += _cardBasic.Height + 14;

            _fuzzyToggle = EnsureFuzzyToggle(_fuzzyToggle, y);
            _fuzzyToggle.Text = _fuzzyShown ? "隐藏模糊音设置 ▲" : "显示模糊音设置 ▼";
            y += _fuzzyToggle.Height + 10;

            // 卡片始终创建（否则按钮会留在窗体上），只是可见性跟着开关走
            {
                _cardFuzzy = EnsureCard(_cardFuzzy, y);
                _cardFuzzy.Visible = _fuzzyShown;
                var fuzzyRows = new System.Collections.Generic.List<object[]>
                {
                    new object[] { "zh, ch, sh => z, c, s",    new Control[] { button8,  button7  } },
                    new object[] { "z, c, s => zh, ch, sh",    new Control[] { button10, button9  } },
                    new object[] { "n => l",                   new Control[] { button14, button13 } },
                    new object[] { "l => n",                   new Control[] { button12, button11 } },
                    new object[] { "r => l",                   new Control[] { button22, button21 } },
                    new object[] { "ren => yin, reng => ying", new Control[] { button20, button19 } },
                    new object[] { "r => y",                   new Control[] { button18, button17 } },
                    new object[] { "hu => fu",                 new Control[] { button16, button15 } },
                    new object[] { "hong => feng",             new Control[] { button28, button27 } },
                    new object[] { "hui => fei, hun => fen",   new Control[] { button26, button25 } },
                    new object[] { "hua => fa",                new Control[] { button24, button23 } },
                    new object[] { "meng = mong",              new Control[] { button50, button49 } },
                    new object[] { "en => eng, in => ing",     new Control[] { button48, button47 } },
                    new object[] { "eng => en, ing => in",     new Control[] { button46, button45 } },
                    new object[] { "un => iong",               new Control[] { button44, button43 } },
                    new object[] { "iong => un",               new Control[] { button42, button41 } },
                    new object[] { "an => ang",                new Control[] { button40, button39 } },
                    new object[] { "ian => iang",              new Control[] { button38, button37 } },
                    new object[] { "uan => uang",              new Control[] { button36, button35 } },
                };
                _cardFuzzy.Height = LayoutRows(_cardFuzzy, fuzzyRows, true);
                if (_fuzzyShown) y += _cardFuzzy.Height + 14;
            }

            _titleMisc = EnsureTitle(_titleMisc, "维护", ref y);
            _cardMisc = EnsureCard(_cardMisc, y);
            PlaceRow(button5, _cardMisc, PadX, 14);
            button5.Size = new Size(90, 28);
            button5.Visible = true;
            PlaceLabel("初始化会把外观恢复成默认主题", _cardMisc, PadX + 102, 19);
            _cardMisc.BackColor = Color.White;
            _cardMisc.Height = 56;

            y += _cardMisc.Height + 12;

            _hint = EnsureHint(_hint, y);
            y += _hint.Height + 10;

            int maxH = Screen.PrimaryScreen.WorkingArea.Height - 90;
            this.AutoScroll = (y > maxH);      // 只有展开后超出屏幕才滚动
            if (y > maxH) y = maxH;
            this.ClientSize = new Size(this.ClientSize.Width, y);
            ApplyAdState();

            ResumeLayout();
        }

        /// <summary>两列各自独立排布，返回卡片高度</summary>
        private int LayoutRows(Panel card, System.Collections.Generic.List<object[]> rows, bool singleColumn = false)
        {
            int half = singleColumn ? rows.Count : (rows.Count + 1) / 2;
            int rowsLeft = half;

            for (int i = 0; i < rows.Count; i++)
            {
                bool left = i < half;
                int idx = left ? i : i - half;
                int y = 14 + idx * RowH;
                int labelX = singleColumn ? PadX : (left ? PadX : RightLabelX);
                int valX = singleColumn ? 210 : (left ? LeftValX : RightValX);

                // 右列标签更长，给它更宽的位子
                PlaceRowWithLabel((string)rows[i][0], (Control[])rows[i][1], card, labelX, valX, y);
            }
            return rowsLeft * RowH + 22;
        }

        private void ToggleFuzzy()
        {
            _fuzzyShown = !_fuzzyShown;
            RebuildLayout();
        }

        private void HideInternal(Control c)
        {
            if (c == null) return;
            c.Visible = false;
        }

        private Label EnsureTitle(Label lbl, string text, ref int y)
        {
            if (lbl == null)
            {
                lbl = new Label();
                this.Controls.Add(lbl);
            }
            lbl.Text = text;
            lbl.AutoSize = true;
            lbl.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(17, 24, 39);
            lbl.Location = new Point(CardX + PadX, y);
            lbl.BringToFront();
            y += lbl.Height + 6;
            return lbl;
        }

        private Panel EnsureCard(Panel card, int y)
        {
            if (card == null)
            {
                card = new Panel();
                card.BackColor = Color.White;
                card.Paint += delegate (object s2, PaintEventArgs e2)
                {
                    Panel self = (Panel)s2;
                    using (Pen p = new Pen(Color.FromArgb(228, 231, 236)))
                        e2.Graphics.DrawRectangle(p, 0, 0, self.Width - 1, self.Height - 1);
                };
                this.Controls.Add(card);
                card.SendToBack();
            }
            card.SetBounds(CardX, y, CardW, 40);
            card.Visible = true;
            card.SendToBack();
            return card;
        }

        /// <summary>两列铺开：返回值是卡片需要的高度</summary>
        

        private void PlaceRowWithLabel(string text, Control[] controls, Panel card, int labelX, int valX, int y)
        {
            PlaceLabel(text, card, labelX, y + 4);
            int x = valX;
            foreach (Control c in controls)
            {
                if (c == null) continue;
                if (!card.Controls.Contains(c)) card.Controls.Add(c);
                c.Left = x;
                c.Top = y;
                if (c is Button) c.Size = new Size(BtnW, BtnH);
                x += c.Width + Gap;
                c.BringToFront();
            }
        }

        private void PlaceRow(Control c, Panel card, int x, int y)
        {
            if (!card.Controls.Contains(c)) card.Controls.Add(c);
            c.Left = x;
            c.Top = y;
            c.BringToFront();
        }

        private void PlaceLabel(string text, Panel card, int x, int y)
        {
            Label lbl = null;
            foreach (Control c in this.Controls)
            {
                Label l = c as Label;
                if (l != null && l.Text == text) { lbl = l; break; }
            }
            if (lbl == null)
            {
                lbl = new Label();
                lbl.Text = text;
                lbl.AutoSize = true;
                lbl.ForeColor = Color.FromArgb(55, 65, 81);
                this.Controls.Add(lbl);
            }
            if (!card.Controls.Contains(lbl)) card.Controls.Add(lbl);
            lbl.Left = x;
            lbl.Top = y;
            lbl.BringToFront();
        }

        private Button EnsureFuzzyToggle(Button btn, int y)
        {
            if (btn == null)
            {
                btn = new Button();
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(208, 213, 221);
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(52, 64, 84);
                btn.Cursor = Cursors.Hand;
                btn.Size = new Size(180, 30);
                btn.Click += delegate { ToggleFuzzy(); };
                this.Controls.Add(btn);
            }
            btn.Text = _fuzzyShown ? "隐藏模糊音设置 ▲" : "显示模糊音设置 ▼";
            btn.Location = new Point(CardX + PadX, y);
            btn.BringToFront();
            return btn;
        }

        private Label EnsureHint(Label lbl, int y)
        {
            if (lbl == null)
            {
                lbl = new Label();
                lbl.AutoSize = true;
                lbl.ForeColor = Color.FromArgb(107, 114, 128);
                this.Controls.Add(lbl);
            }
            lbl.Text = "所有设置点击按钮后即时生效";
            lbl.Location = new Point(CardX + PadX, y);
            lbl.BringToFront();
            return lbl;
        }

        


        private static readonly string[] PredMark = { "  # >>> 联想提示（自动生成，勿手改）", "  # <<< 联想提示" };

        /// <summary>联想数据包放在用户目录（引擎按用户目录查找 predict.db）</summary>
        private string PredictDbPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                    "Rime", "predict.db");
            }
        }

        /// <summary>
        /// 用户级补丁必须写在用户目录（%APPDATA%\Rime），不能写安装目录：
        /// 安装目录受 UAC 保护，而且会被重新部署覆盖，改错了还会影响 64 位引擎加载。
        /// </summary>
        private string CustomYamlPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                    "Rime", "xiaobai_simp.custom.yaml");
            }
        }

        private bool IsPredictEnabled()
        {
            try { return System.IO.File.Exists(CustomYamlPath) && System.IO.File.ReadAllText(CustomYamlPath).Contains(PredMark[0].Trim()); }
            catch { return false; }
        }

        /// <summary>
        /// 联想提示：往 xiaobai_simp.custom.yaml 的 patch 里插入/移除一段补丁。
        /// 插入后需要重新部署；没有 predict.db 时引擎会打日志但不会崩。
        /// </summary>
        private void SetPredict(bool on)
        {
            try
            {
                string p = CustomYamlPath;
                var lines = System.IO.File.Exists(p)
                    ? new System.Collections.Generic.List<string>(System.IO.File.ReadAllLines(p))
                    : new System.Collections.Generic.List<string>(new[] { "patch:" });
                int begin = lines.FindIndex(l => l.Trim() == PredMark[0].Trim());
                int end = lines.FindIndex(l => l.Trim() == PredMark[1].Trim());
                if (begin >= 0 && end > begin) lines.RemoveRange(begin, end - begin + 1);

                if (on)
                {
                    int patchAt = lines.FindIndex(l => l.TrimEnd() == "patch:");
                    if (patchAt < 0) { lines.Add("patch:"); patchAt = lines.Count - 1; }

                    var block = new System.Collections.Generic.List<string>
                    {
                        PredMark[0],
                        "  \"engine/processors/@before 0\": predictor",
                        "  \"engine/translators/@before 0\": predict_translator",
                        "  predictor:",
                        "    db: predict.db",
                        "    max_candidates: 5",
                        "    max_iterations: 0",   // 0 = 不限次数，可以一直联想下去
                        PredMark[1]
                    };
                    lines.InsertRange(patchAt + 1, block);
                }

                System.IO.File.WriteAllLines(p, lines.ToArray());
                DeployNow();
            }
            catch { }
        }

        private void BuildPredictSwitch()
        {
            _chkPredict = new CheckBox();
            _chkPredict.AutoSize = true;
            _chkPredict.ForeColor = Color.FromArgb(55, 65, 81);

            // 没有数据包时不给勾：勾了也不会生效，只会让人以为坏了
            bool hasDb = System.IO.File.Exists(PredictDbPath);
            _chkPredict.Enabled = hasDb;
            _chkPredict.Text = "打完后提示下一词";
            _chkPredict.ForeColor = hasDb ? Color.FromArgb(55, 65, 81) : Color.FromArgb(156, 163, 175);
            if (!hasDb)
            {
                _tip = new ToolTip();
                _tip.SetToolTip(_chkPredict, "还没有安装联想数据包（predict.db），暂时不可用");
            }
            _chkPredict.Checked = hasDb && IsPredictEnabled();
            _chkPredict.CheckedChanged += delegate { SetPredict(_chkPredict.Checked); };
            this.Controls.Add(_chkPredict);
        }


        /// <summary>统一字号为微软雅黑 9pt，去掉宋体 12px 的“老软件”观感。</summary>
        private void UnifyFonts(Control parent)
        {
            Font ui = new Font("Microsoft YaHei UI", 9F);
            foreach (Control c in parent.Controls)
            {
                if (c is Label || c is Button || c is CheckBox
                    || c is TextBox || c is ComboBox || c is RadioButton)
                {
                    c.Font = ui;
                }
                if (c.HasChildren) UnifyFonts(c);
            }
        }

        private void StyleButtons(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                Button btn = c as Button;
                if (btn != null && btn != btnToggleAd)
                {
                    btn.UseVisualStyleBackColor = false;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(208, 213, 221);
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 244, 250);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(226, 234, 246);
                    btn.BackColor = Color.White;
                    btn.ForeColor = Color.FromArgb(52, 64, 84);
                    btn.Cursor = Cursors.Hand;
                }
                ComboBox cb = c as ComboBox;
                if (cb != null)
                {
                    cb.FlatStyle = FlatStyle.Flat;
                    cb.BackColor = Color.White;
                    cb.ForeColor = Color.FromArgb(52, 64, 84);
                }
                TextBox tb = c as TextBox;
                if (tb != null)
                {
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.BackColor = Color.White;
                    tb.ForeColor = Color.FromArgb(52, 64, 84);
                }
                if (c.HasChildren) StyleButtons(c);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hxsl1.Text = "9";
            //快捷方式文件的路径 = @"d:\Test.lnk";
            // 不再依赖开始菜单里的旧快捷方式（2026.11.11 起快捷方式已重排），
            // 直接由本程序所在目录推出安装目录与部署程序
            快捷方式文件指向的目标目录.Text = InstallDir;
            string deployerPath = Path.Combine(InstallDir, "WeaselDeployer.exe");
            if (!System.IO.File.Exists(deployerPath))
                deployerPath = Path.Combine(InstallDir, @"Win32\WeaselDeployer.exe");
            快捷方式文件指向的路径.Text = deployerPath;


            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            try { string p = Path.Combine(appDir, "wechat_qr.png"); if (System.IO.File.Exists(p)) qrWechat.Image = Image.FromFile(p); } catch { }
            try { string p = Path.Combine(appDir, "alipay_qr.png"); if (System.IO.File.Exists(p)) qrAlipay.Image = Image.FromFile(p); } catch { }
            try { string p = Path.Combine(appDir, "taobao_qrcode.png"); if (System.IO.File.Exists(p)) qrTaobao.Image = Image.FromFile(p); } catch { }

            // Restore ad panel state
            try { if (System.IO.File.Exists(_adStateFile) && System.IO.File.ReadAllText(_adStateFile).Trim() == "1") _adCollapsed = true; } catch { }
            ApplyAdState();

            ApplyModernTheme();
        }

        /// <summary>本程序所在目录，即输入法安装目录</summary>
        private static string InstallDir
        {
            get { return AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'); }
        }

        /// <summary>调用 WeaselDeployer 重新部署，不再依赖开始菜单快捷方式</summary>
        private Process _deployProc;
        private string _lastDeployHash;
        private Panel _deployPanel;
        private Label _deployLabel;
        private Label _deployTitle;
        private ProgressBar _deployBar;
        private Timer _deployTimer;
        private Timer _deployHideTimer;

        /// <summary>
        /// 改完设置要重新部署（大约一分钟，期间打不了字）。
        /// 跑起来之后弹一层遮罩，让用户知道在忙、不是坏了；部署完成再告诉他可以打字了。
        /// </summary>
        private void DeployNow()
        {
            try
            {
                string deployer = Path.Combine(InstallDir, "WeaselDeployer.exe");
                if (!System.IO.File.Exists(deployer))
                    deployer = Path.Combine(InstallDir, @"Win32\WeaselDeployer.exe");
                if (!System.IO.File.Exists(deployer)) return;

                if (_deployProc != null && !SafeHasExited(_deployProc))
                {
                    ShowDeployOverlay();      // 已经在部署，重复点击只刷新提示
                    return;
                }

                // 设置没变就别部署：重建 prism 要三十秒，白等一次很浪费
                string hash = SchemaStamp();
                if (hash != null && hash == _lastDeployHash)
                {
                    ShowNothingChanged();
                    return;
                }
                _lastDeployHash = hash;

                _deployProc = Process.Start(new ProcessStartInfo(deployer, "/deploy")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(deployer)
                });

                ShowDeployOverlay();
                StartDeployWatch();
            }
            catch
            {
                HideDeployOverlay();
            }
        }

        private static bool SafeHasExited(Process p)
        {
            try { return p.HasExited; } catch { return true; }
        }

        private void ShowDeployOverlay()
        {
            if (_deployPanel == null)
            {
                _deployPanel = new Panel();
                // 用绝对尺寸而不是 Dock：窗体开了 AutoScroll，Dock=Fill 会落进滚动区里
                _deployPanel.Bounds = this.ClientRectangle;
                _deployPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                _deployPanel.BackColor = Color.FromArgb(250, 250, 251);

                var title = new Label();
                _deployTitle = title;
                title.Text = "正在重新部署输入法…";
                title.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
                title.ForeColor = Color.FromArgb(17, 24, 39);
                title.AutoSize = false;
                title.TextAlign = ContentAlignment.MiddleCenter;
                title.SetBounds(0, 0, 600, 46);

                _deployLabel = new Label();
                _deployLabel.Text = "大约需要 1 分钟，这段时间不能打字，请稍等";
                _deployLabel.Font = new Font("Microsoft YaHei UI", 10F);
                _deployLabel.ForeColor = Color.FromArgb(107, 114, 128);
                _deployLabel.AutoSize = false;
                _deployLabel.TextAlign = ContentAlignment.MiddleCenter;
                _deployLabel.SetBounds(0, 0, 600, 26);

                _deployBar = new ProgressBar();
                _deployBar.Style = ProgressBarStyle.Marquee;
                _deployBar.MarqueeAnimationSpeed = 30;
                _deployBar.SetBounds(0, 0, 260, 10);

                _deployPanel.Controls.Add(title);
                _deployPanel.Controls.Add(_deployLabel);
                _deployPanel.Controls.Add(_deployBar);
                this.Controls.Add(_deployPanel);

                _deployPanel.Resize += delegate { LayoutOverlay(); };
            }

            _deployTitle.Text = "正在重新部署输入法…";
            _deployTitle.ForeColor = Color.FromArgb(17, 24, 39);
            _deployBar.Visible = true;
            _deployBar.Style = ProgressBarStyle.Marquee;
            _deployLabel.ForeColor = Color.FromArgb(107, 114, 128);
            _deployLabel.Text = "大约需要 1 分钟，这段时间不能打字，请稍等";
            _deployPanel.Bounds = this.ClientRectangle;
            LayoutOverlay();
            _deployPanel.Visible = true;
            this.Controls.SetChildIndex(_deployPanel, 0);
            _deployPanel.BringToFront();
            _deployPanel.PerformLayout();
        }

        private void LayoutOverlay()
        {
            if (_deployPanel == null || _deployTitle == null) return;
            int cx = _deployPanel.Width / 2;
            int cy = _deployPanel.Height / 2;
            _deployTitle.SetBounds(cx - 300, cy - 74, 600, 46);
            _deployLabel.SetBounds(cx - 300, cy - 20, 600, 26);
            _deployBar.SetBounds(cx - 130, cy + 22, 260, 12);
        }

        /// <summary>把各方案文件的内容算成一个指纹，用来判断这次点击有没有真的改变设置</summary>
        private string SchemaStamp()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                string dir = Path.Combine(InstallDir, "data");
                if (!Directory.Exists(dir)) return null;
                foreach (string f in Directory.GetFiles(dir, "*.schema.yaml"))
                {
                    // 必须比内容而不是时间戳：按钮无论如何都会重写文件，时间戳每次都变
                    byte[] bytes = System.IO.File.ReadAllBytes(f);
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                        sb.Append(f).Append(':')
                          .Append(BitConverter.ToString(md5.ComputeHash(bytes)))
                          .Append(';');
                }
                return sb.ToString();
            }
            catch { return null; }
        }

        private void ShowNothingChanged()
        {
            ShowDeployOverlay();
            _deployTitle.Text = "设置没有变化";
            _deployTitle.ForeColor = Color.FromArgb(107, 114, 128);
            _deployBar.Visible = false;
            _deployLabel.ForeColor = Color.FromArgb(107, 114, 128);
            _deployLabel.Text = "这项设置已经是当前值，不用重新部署";
            if (_deployHideTimer == null)
            {
                _deployHideTimer = new Timer();
                _deployHideTimer.Interval = 1800;
                _deployHideTimer.Tick += delegate { _deployHideTimer.Stop(); HideDeployOverlay(); };
            }
            _deployHideTimer.Start();
        }

        private void StartDeployWatch()
        {
            if (_deployTimer == null)
            {
                _deployTimer = new Timer();
                _deployTimer.Interval = 400;
                _deployTimer.Tick += delegate
                {
                    if (_deployProc == null || !SafeHasExited(_deployProc)) return;
                    _deployTimer.Stop();
                    bool ok;
                    try { ok = _deployProc.ExitCode == 0; } catch { ok = true; }
                    FinishDeploy(ok);
                };
            }
            _deployTimer.Start();
        }

        private void FinishDeploy(bool ok)
        {
            if (_deployPanel == null || !_deployPanel.Visible) return;

            _deployBar.Visible = false;
            _deployTitle.Text = ok ? "部署完成" : "部署没有成功";
            _deployTitle.ForeColor = ok ? Color.FromArgb(16, 122, 87) : Color.FromArgb(180, 60, 60);
            _deployLabel.ForeColor = ok ? Color.FromArgb(16, 122, 87) : Color.FromArgb(180, 60, 60);
            _deployLabel.Text = ok
                ? "✔ 现在可以打字了"
                : "⚠ 请到「高级工具 → 重新部署」再试一次";

            if (_deployHideTimer == null)
            {
                _deployHideTimer = new Timer();
                _deployHideTimer.Interval = 2500;
                _deployHideTimer.Tick += delegate
                {
                    _deployHideTimer.Stop();
                    HideDeployOverlay();
                };
            }
            _deployHideTimer.Start();
        }

        private void HideDeployOverlay()
        {
            if (_deployPanel != null) _deployPanel.Visible = false;
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            string strFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, 快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml");
            if (System.IO.File.Exists(strFilePath))
            {
                string strContent = System.IO.File.ReadAllText(strFilePath);
                strContent = Regex.Replace(strContent, "  spelling_hints: 0", "  spelling_hints: 99");
                System.IO.File.WriteAllText(strFilePath, strContent);
            }
            DeployNow();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string strFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, 快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml");
            if (System.IO.File.Exists(strFilePath))
            {
                string strContent = System.IO.File.ReadAllText(strFilePath);
                strContent = Regex.Replace(strContent, "  spelling_hints: 99", "  spelling_hints: 0");
                System.IO.File.WriteAllText(strFilePath, strContent);
            }
            DeployNow();

        }

        private void button3_Click(object sender, EventArgs e)
        {
             string strFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, appdata + @"\Rime\weasel.custom.yaml");
                if (System.IO.File.Exists(strFilePath))
                {
                    string strContent = System.IO.File.ReadAllText(strFilePath);
                    strContent = Regex.Replace(strContent, "  \"style/horizontal\": false", "  \"style/horizontal\": true");
                    System.IO.File.WriteAllText(strFilePath, strContent);
                }
                DeployNow();

            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string strFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, appdata + @"\Rime\weasel.custom.yaml");
            if (System.IO.File.Exists(strFilePath))
            {
                string strContent = System.IO.File.ReadAllText(strFilePath);
                strContent = Regex.Replace(strContent, "  \"style/horizontal\": true", "  \"style/horizontal\": false");
                System.IO.File.WriteAllText(strFilePath, strContent);
            }
            DeployNow();


        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            FileStream stream = System.IO.File.Open(appdata+@"\Rime\weasel.custom.yaml", FileMode.OpenOrCreate, FileAccess.Write);
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            stream.Close();


            StreamWriter sw = new StreamWriter(appdata + @"\Rime\weasel.custom.yaml", true, Encoding.GetEncoding("UTF-8"));
            sw.WriteLine("customization:\n  distribution_code_name: Weasel\n  distribution_version: 0.14.3\n  generator: \"Weasel::UIStyleSettings\"\n  modified_time: \"Thu Oct 22 15:46:29 2020\"\n  rime_version: 1.5.3\npatch:\n  \"style/color_scheme\": dota_2\n  \"style/horizontal\": false");
            sw.Flush();
            sw.Close();
            DeployNow();









        }

        private void button6_Click(object sender, EventArgs e)
        {
            FileStream stream = System.IO.File.Open(appdata + @"\Rime\default.custom.yaml", FileMode.OpenOrCreate, FileAccess.Write);
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            stream.Close();

            StreamWriter sw = new StreamWriter(appdata + @"\Rime\default.custom.yaml", true, Encoding.GetEncoding("UTF-8"));
            sw.WriteLine("customization:\n  distribution_code_name: Weasel\n  distribution_version: 0.14.3\n  generator: \"Rime::SwitcherSettings\"\n  modified_time: \"Fri Aug  7 15:00:14 2020\"\n  rime_version: 1.5.3\npatch:\n  menu/page_size: "+ hxsl1.Text +"\n  schema_list:\n    - {schema: xiaobai_simp}");
            sw.Flush();
            sw.Close();
            DeployNow();


            DeployNow();

        }

        private void button8_Click(object sender, EventArgs e)
        {


            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^([zcs])h/$1/", @" - derive/^([zcs])h/$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^([zcs])h/$1/", @" #- derive/^([zcs])h/$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button10_Click(object sender, EventArgs e)
        {


            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^([zcs])([^h])/$1h$2/", @" - derive/^([zcs])([^h])/$1h$2/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();

        }

        private void button9_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^([zcs])([^h])/$1h$2/", @" #- derive/^([zcs])([^h])/$1h$2/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button14_Click(object sender, EventArgs e)
        {

            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^n/l/", @" - derive/^n/l/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();


        }

        private void button13_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^n/l/", @" #- derive/^n/l/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();

        }

        private void button12_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^l/n/", @" - derive/^l/n/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^l/n/", @" #- derive/^l/n/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button22_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^r/l/", @" - derive/^r/l/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^r/l/", @" #- derive/^r/l/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^ren/yin/", @" - derive/^ren/yin/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^ren/yin/", @" #- derive/^ren/yin/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^r/y/", @" - derive/^r/y/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^r/y/", @" #- derive/^r/y/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^hu$/fu/", @" - derive/^hu$/fu/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^hu$/fu/", @" #- derive/^hu$/fu/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button28_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^hong$/feng/", @" - derive/^hong$/feng/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^hong$/feng/", @" #- derive/^hong$/feng/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^hu([in])$/fe$1/", @" - derive/^hu([in])$/fe$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button25_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^hu([in])$/fe$1/", @" #- derive/^hu([in])$/fe$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^f([ao])/hu$1/", @" - derive/^f([ao])/hu$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^f([ao])/hu$1/", @" - derive/^f([ao])/hu$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button50_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^([bpmf])eng$/$1ong/", @" - derive/^([bpmf])eng$/$1ong/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button49_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^([bpmf])eng$/$1ong/", @" #- derive/^([bpmf])eng$/$1ong/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button48_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/([ei])n$/$1ng/", @" - derive/([ei])n$/$1ng/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button47_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/([ei])n$/$1ng/", @" #- derive/([ei])n$/$1ng/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button46_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/([ei])ng$/$1n/", @" - derive/([ei])ng$/$1n/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button45_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/([ei])ng$/$1n/", @" #- derive/([ei])ng$/$1n/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button44_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^un/iong/", @" - derive/^un/iong/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button43_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^un/iong/", @" #- derive/^un/iong/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button42_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^iong/un/", @" - derive/^iong/un/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button41_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^iong/un/", @" #- derive/^iong/un/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button40_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^an/ang/", @" - derive/^an/ang/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button39_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^an/ang/", @" #- derive/^an/ang/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button38_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^ian/iang/", @" - derive/^ian/iang/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button37_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^ian/iang/", @" #- derive/^ian/iang/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button36_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" #- derive/^uan/uang/", @" - derive/^uan/uang/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button35_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@" - derive/^uan/uang/", @" #- derive/^uan/uang/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button32_Click(object sender, EventArgs e)
        {

        }

        private void button29_Click(object sender, EventArgs e)
        {
            string strFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, appdata + @"\Rime\build\weasel.yaml");
            if (System.IO.File.Exists(strFilePath))
            {
                string strContent = System.IO.File.ReadAllText(strFilePath);
                strContent = Regex.Replace(strContent, "  font_point: .*\r\n", "  font_point: "+hxdx1.Text+"\r\n");
                System.IO.File.WriteAllText(strFilePath, strContent);
            }
            DeployNow();

        }

        private void button31_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"- xform/;/7/", @"#- xform/;/7/");
                    text = text.Replace(@"- xform/^;/7/", @"#- xform/^;/7/");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                    text = text.Replace(@"#delimiter: ""7""", @"delimiter: ""7""");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();

        }

        private void button30_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    text = text.Replace(@"#- xform/;/7/", @"- xform/;/7/");
                    text = text.Replace(@"#- xform/^;/7/", @"- xform/^;/7/");
                    //多删几遍,防止出现多个#号
                    text = text.Replace(@"delimiter: ""7""", @"#delimiter: ""7""");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button33_Click(object sender, EventArgs e)
        {

            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"- {accept: Left, send: Left, when: has_menu}", @"- {accept: Left, send: Page_Up, when: has_menu}");
                    text = text.Replace(@"- {accept: Right, send: Right, when: has_menu}", @"- {accept: Right, send: Page_Down, when: has_menu}");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button32_Click_1(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"- {accept: Left, send: Page_Up, when: has_menu}", @"- {accept: Left, send: Left, when: has_menu}");
                    text = text.Replace(@"- {accept: Right, send: Page_Down, when: has_menu}", @"- {accept: Right, send: Right, when: has_menu}");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button51_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai.dict.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"  #- dicts/ywz", @"  - dicts/ywz");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai.dict.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button34_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai.dict.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"  - dicts/ywz", @"  #- dicts/ywz");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai.dict.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button53_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.custom.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"    #simplifier@emoji_suggestion", @"    simplifier@emoji_suggestion");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.custom.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button52_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.custom.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"    simplifier@emoji_suggestion", @"    #simplifier@emoji_suggestion");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.custom.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button55_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"    #- derive/^([a-z]{1})([a-z])$/$1/", @"    - derive/^([a-z]{1})([a-z])$/$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button54_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"    - derive/^([a-z]{1})([a-z])$/$1/", @"    #- derive/^([a-z]{1})([a-z])$/$1/");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void label18_MouseEnter(object sender, EventArgs e)
        {
            候选大小tip.Show("默认14", (Label)sender);
        }

        private void button29_MouseEnter(object sender, EventArgs e)
        {
            候选大小tip.Show("默认14", (Button)sender);
        }

        private void label34_MouseEnter(object sender, EventArgs e)
        {
            多一码候选tip.Show("没特殊要求不建议开启", (Label)sender);
        }

        private void button55_MouseEnter(object sender, EventArgs e)
        {
            多一码候选tip.Show("没特殊要求不建议开启", (Button)sender);
        }

        private void button57_Click(object sender, EventArgs e)
        {// 注意：如果是针对特定方案，建议修改 xiaobai_simp.custom.yaml；如果是全局，则修改 default.custom.yaml
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"  enable_completion: false", @"  enable_completion: true");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void button56_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();
                    text = text.Replace(@"  enable_completion: true", @"  enable_completion: false");
                }
            }

            using (FileStream fs = new FileStream(快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void hxzs1_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            string filePath = 快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml";


            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();

                    // 🎯 【核心修复】：匹配行首任意组合的空格和#号，一律替换为规范的带#号注释
                    text = Regex.Replace(text,
                        @"^[ \t#]*-\s*lua_filter@\*t9_preedit",
                        @"    #- lua_filter@*t9_preedit",
                        RegexOptions.Multiline);
                }
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void hxzs2_Click(object sender, EventArgs e)
        {
            string text = string.Empty;
            string filePath = 快捷方式文件指向的目标目录.Text + "\\data\\xiaobai_simp.schema.yaml";


            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    text = sr.ReadToEnd();

                    // 🎯 【核心修复】：匹配行首任意组合的空格和#号，一律替换为干净放行的核心代码
                    text = Regex.Replace(text,
                        @"^[ \t#]*-\s*lua_filter@\*t9_preedit",
                        @"    - lua_filter@*t9_preedit",
                        RegexOptions.Multiline);
                }
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(text);
                }
            }

            DeployNow();
        }

        private void adPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using (var tf = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold))
            using (var tb = new SolidBrush(Color.FromArgb(200, 80, 40)))
            {
                var w = g.MeasureString("支持小白", tf).Width;
                g.DrawString("支持小白", tf, tb, (190 - w) / 2, 8);
            }
            using (var lf = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold))
            using (var sf = new Font("Microsoft YaHei UI", 7.5F))
            using (var gb = new SolidBrush(Color.FromArgb(80, 80, 80)))
            using (var db = new SolidBrush(Color.FromArgb(50, 50, 50)))
            using (var rb = new SolidBrush(Color.FromArgb(210, 50, 20)))
            using (var pen = new Pen(Color.FromArgb(230, 210, 190), 1))
            {
                var w1 = g.MeasureString("微信打赏", lf).Width;
                g.DrawString("微信打赏", lf, db, (190 - w1) / 2, 148);
                var w1d = g.MeasureString("感谢支持小白输入法", sf).Width;
                g.DrawString("感谢支持小白输入法", sf, gb, (190 - w1d) / 2, 168);
                g.DrawLine(pen, 20, 200, 170, 200);
                var w2 = g.MeasureString("支付宝打赏", lf).Width;
                g.DrawString("支付宝打赏", lf, db, (190 - w2) / 2, 358);
                var w2d = g.MeasureString("感谢支持小白输入法", sf).Width;
                g.DrawString("感谢支持小白输入法", sf, gb, (190 - w2d) / 2, 378);
                g.DrawLine(pen, 20, 418, 170, 418);
                var w3 = g.MeasureString("淘宝店铺", lf).Width;
                g.DrawString("淘宝店铺", lf, db, (190 - w3) / 2, 578);
                var w3a = g.MeasureString("捐助98元可赠送", sf).Width;
                g.DrawString("捐助98元可赠送", sf, gb, (190 - w3a) / 2, 598);
                var w3b = g.MeasureString("小白T9无线键盘一台!", sf).Width;
                g.DrawString("小白T9无线键盘一台!", sf, rb, (190 - w3b) / 2, 614);
            }
        }


        private void btnToggleAd_Click(object sender, EventArgs e)
        {
            _adCollapsed = !_adCollapsed;
            ApplyAdState();
            try { System.IO.File.WriteAllText(_adStateFile, _adCollapsed ? "1" : "0"); } catch { }
        }

        private void ApplyAdState()
        {
            // 高度用当前值，不要写死 687：展开/收起模糊音时窗口要能变高变矮
            int h = this.ClientSize.Height;
            adPanel.Height = h;
            btnToggleAd.Location = new System.Drawing.Point(625, 300);
            if (_adCollapsed)
            {
                adPanel.Visible = false;
                btnToggleAd.Text = ">";
                this.ClientSize = new System.Drawing.Size(645, h);
            }
            else
            {
                adPanel.Visible = true;
                btnToggleAd.Text = "<";
                this.ClientSize = new System.Drawing.Size(845, h);
            }
        }
    }
}
