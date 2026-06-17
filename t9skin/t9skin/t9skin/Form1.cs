using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using static System.IO.File;
using static System.Drawing.ColorTranslator;

namespace t9skin
{
    public partial class t9skin : Form
    {
        public t9skin()
        {
            InitializeComponent();
            c1.Click += (e, a) => C_Click(c1);
            c2.Click += (e, a) => C_Click(c2);
            c3.Click += (e, a) => C_Click(c3);
            c4.Click += (e, a) => C_Click(c4);
            c5.Click += (e, a) => C_Click(c5);
            c6.Click += (e, a) => C_Click(c6);
            c7.Click += (e, a) => C_Click(c7);
            c8.Click += (e, a) => C_Click(c8);
            c9.Click += (e, a) => C_Click(c9);
            c10.Click += (e, a) => C_Click(c10);
            c11.Click += (e, a) => C_Click(c11);
            c12.Click += (e, a) => C_Click(c12);
            c13.Click += (e, a) => C_Click(c13);
        }

        public static string yaml = System.Windows.Forms.Application.StartupPath + "\\data\\weasel.yaml";

        public static string Reverse(string text)
        {
            char[] cArray = text.ToCharArray();
            string reverse = String.Empty;
            reverse += cArray[6];
            reverse += cArray[7];
            reverse += cArray[4];
            reverse += cArray[5];
            reverse += cArray[2];
            reverse += cArray[3];
            reverse += cArray[0];
            reverse += cArray[1];
            return reverse;
        }

        public static string ToHexValue(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
        }

        public string ReverseHex(Color color)
        {
            return Reverse(ToHexValue(color).Substring(ToHexValue(color).Length - 8, 8));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] lines = ReadAllLines(yaml);
            int xiaobaiIndex = Array.FindIndex(lines, l => l.Trim() == "xiaobai:");
            if (xiaobaiIndex == -1)
            {
                MessageBox.Show("未能找到 xiaobai 配置。");
                return;
            }

            string text_color = lines[xiaobaiIndex + 2];
            string back_color = lines[xiaobaiIndex + 3];
            string shadow_color = lines[xiaobaiIndex + 4];
            string border_color = lines[xiaobaiIndex + 5];
            string hilited_text_color = lines[xiaobaiIndex + 6];
            string hilited_back_color = lines[xiaobaiIndex + 7];
            string hilited_shadow_color = lines[xiaobaiIndex + 8];
            string hilited_candidate_text_color = lines[xiaobaiIndex + 9];
            string hilited_candidate_back_color = lines[xiaobaiIndex + 10];
            string hilited_candidate_shadow_color = lines[xiaobaiIndex + 11];
            string candidate_text_color = lines[xiaobaiIndex + 12];
            string candidate_back_color = lines[xiaobaiIndex + 13];
            string candidate_shadow_color = lines[xiaobaiIndex + 14];

            string text_colorz = "#" + Reverse(text_color.Substring(text_color.Length - 8, 8));
            string back_colorz = "#" + Reverse(back_color.Substring(back_color.Length - 8, 8));
            string shadow_colorz = "#" + Reverse(shadow_color.Substring(shadow_color.Length - 8, 8));
            string border_colorz = "#" + Reverse(border_color.Substring(border_color.Length - 8, 8));
            string hilited_text_colorz = "#" + Reverse(hilited_text_color.Substring(hilited_text_color.Length - 8, 8));
            string hilited_back_colorz = "#" + Reverse(hilited_back_color.Substring(hilited_back_color.Length - 8, 8));
            string hilited_shadow_colorz = "#" + Reverse(hilited_shadow_color.Substring(hilited_shadow_color.Length - 8, 8));
            string hilited_candidate_text_colorz = "#" + Reverse(hilited_candidate_text_color.Substring(hilited_candidate_text_color.Length - 8, 8));
            string hilited_candidate_back_colorz = "#" + Reverse(hilited_candidate_back_color.Substring(hilited_candidate_back_color.Length - 8, 8));
            string hilited_candidate_shadow_colorz = "#" + Reverse(hilited_candidate_shadow_color.Substring(hilited_candidate_shadow_color.Length - 8, 8));
            string candidate_text_colorz = "#" + Reverse(candidate_text_color.Substring(candidate_text_color.Length - 8, 8));
            string candidate_back_colorz = "#" + Reverse(candidate_back_color.Substring(candidate_back_color.Length - 8, 8));
            string candidate_shadow_colorz = "#" + Reverse(candidate_shadow_color.Substring(candidate_shadow_color.Length - 8, 8));

            c1.BackColor = FromHtml(text_colorz.Substring(0, 7));
            c2.BackColor = FromHtml(back_colorz.Substring(0, 7));
            c3.BackColor = FromHtml(shadow_colorz.Substring(0, 7));
            c4.BackColor = FromHtml(border_colorz.Substring(0, 7));
            c5.BackColor = FromHtml(hilited_text_colorz.Substring(0, 7));
            c6.BackColor = FromHtml(hilited_back_colorz.Substring(0, 7));
            c7.BackColor = FromHtml(hilited_shadow_colorz.Substring(0, 7));
            c8.BackColor = FromHtml(hilited_candidate_text_colorz.Substring(0, 7));
            c9.BackColor = FromHtml(hilited_candidate_back_colorz.Substring(0, 7));
            c10.BackColor = FromHtml(hilited_candidate_shadow_colorz.Substring(0, 7));
            c11.BackColor = FromHtml(candidate_text_colorz.Substring(0, 7));
            c12.BackColor = FromHtml(candidate_back_colorz.Substring(0, 7));
            c13.BackColor = FromHtml(candidate_shadow_colorz.Substring(0, 7));

            string track_Bar1 = lines.First(l => l.Contains("shadow_radius:"));
            string track_Bar2 = lines.First(l => l.Contains("shadow_offset_x:"));
            string track_Bar3 = lines.First(l => l.Contains("shadow_offset_y:"));
            trackBar1.Value = int.Parse(Regex.Match(track_Bar1, @"\d+").Value);
            trackBar2.Value = int.Parse(Regex.Match(track_Bar2, @"\d+").Value);
            trackBar3.Value = int.Parse(Regex.Match(track_Bar3, @"\d+").Value);

            SetColor();
        }

        private void SetColor()
        {
            //候选
            text.ForeColor = c1.BackColor;
            text.BackColor = c2.BackColor;
            back.BackColor = c2.BackColor;
            border.BackColor = c4.BackColor;
            //编码
            hilited.ForeColor = c5.BackColor;
            hilited.BackColor = c6.BackColor;
            //高亮
            hilited_candidate.ForeColor = c8.BackColor;
            comment0.BackColor = label17.BackColor = hilited_candidate.BackColor = c9.BackColor;
            //非高亮
            candidate1.ForeColor = candidate2.ForeColor = candidate3.ForeColor = candidate4.ForeColor = candidate5.ForeColor = candidate6.ForeColor = c11.BackColor;
            Color originalColor = c11.BackColor;
            int RR = c11.BackColor.R / 2;
            int GG = c11.BackColor.G / 2;
            int BB = c11.BackColor.B / 2;
            if (RR == 0) RR = -128; if (GG == 0) GG = -128; if (BB == 0) BB = -128;
            Color newColor = Color.FromArgb(originalColor.A, originalColor.R - RR, originalColor.G - GG, originalColor.B - BB);
            label17.ForeColor = label18.ForeColor = label19.ForeColor = label20.ForeColor = label21.ForeColor = label22.ForeColor = label23.ForeColor = comment0.ForeColor = comment1.ForeColor = comment2.ForeColor = comment3.ForeColor = comment4.ForeColor = comment5.ForeColor = comment6.ForeColor = newColor;
            label18.BackColor = label19.BackColor = label20.BackColor = label21.BackColor = label22.BackColor = label23.BackColor = comment1.BackColor = comment2.BackColor = comment3.BackColor = comment4.BackColor = comment5.BackColor = comment6.BackColor = candidate1.BackColor = candidate2.BackColor = candidate3.BackColor = candidate4.BackColor = candidate5.BackColor = candidate6.BackColor = c12.BackColor;
        }

        public void RestartProgress()
        {
            try
            {
                string lnkPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\小白T9输入法\【小白T9输入法】重新部署.lnk";
                if (File.Exists(lnkPath))
                {
                    Process.Start(lnkPath);
                }
                else
                {
                    // 如果找不到快捷方式，尝试直接启动同目录下的执行文件
                    string exePath = Path.Combine(Application.StartupPath, "WeaselDeployer.exe");
                    if (File.Exists(exePath))
                    {
                        Process.Start(exePath, "/deploy");
                    }
                }
            }
            catch { /* 默默失败或提示用户手动重启 */ }
        }

        public void CC_Click(Object sender, EventArgs e)
        {
        }

        private void C_Click(Button button)
        {
            Form2 form2 = new Form2();
            try
            {
                // 只有用户点了“确定”，才去执行后续的保存和重启
                if (form2.ShowDialog() == DialogResult.OK)
                {
                    button.BackColor = form2.tsbColor;

                    // 将文件操作放入 try 块中，防止权限不足或文件被占用导致崩溃
                    string[] linesArray = File.ReadAllLines(yaml);
                    List<string> lines = new List<string>(linesArray);

                    int xiaobaiIndex = Array.FindIndex(linesArray, l => l.Trim() == "xiaobai:");
                    if (xiaobaiIndex == -1)
                    {
                        MessageBox.Show("未能找到 xiaobai 配置。");
                        return;
                    }

                    lines[xiaobaiIndex + 2] = "    text_color: 0x" + ReverseHex(c1.BackColor);
            lines[xiaobaiIndex + 3] = "    back_color: 0x" + ReverseHex(c2.BackColor);
            lines[xiaobaiIndex + 4] = "    shadow_color: 0x" + ReverseHex(c3.BackColor);
            lines[xiaobaiIndex + 5] = "    border_color: 0x" + ReverseHex(c4.BackColor);
            lines[xiaobaiIndex + 6] = "    hilited_text_color: 0x" + ReverseHex(c5.BackColor);
            lines[xiaobaiIndex + 7] = "    hilited_back_color: 0x" + ReverseHex(c6.BackColor);
            lines[xiaobaiIndex + 8] = "    hilited_shadow_color: 0x" + ReverseHex(c7.BackColor);
            lines[xiaobaiIndex + 9] = "    hilited_candidate_text_color: 0x" + ReverseHex(c8.BackColor);
            lines[xiaobaiIndex + 10] = "    hilited_candidate_back_color: 0x" + ReverseHex(c9.BackColor);
            lines[xiaobaiIndex + 11] = "    hilited_candidate_shadow_color: 0x" + ReverseHex(c10.BackColor);
            lines[xiaobaiIndex + 12] = "    candidate_text_color: 0x" + ReverseHex(c11.BackColor);
            lines[xiaobaiIndex + 13] = "    candidate_back_color: 0x" + ReverseHex(c12.BackColor);
            lines[xiaobaiIndex + 14] = "    candidate_shadow_color: 0x" + ReverseHex(c13.BackColor);

                    File.WriteAllLines(yaml, lines.ToArray());

                    SetColor();
                    RestartProgress();
                }
            }
            catch (Exception ex)
            {
                // 捕捉所有潜在错误（文件权限、进程启动失败等），避免程序直接崩掉
                MessageBox.Show("操作失败：" + ex.Message, "错误提示");
            }
            finally
            {
                form2.Dispose();
            }
        }

        private void recovery_Click(object sender, EventArgs e)
        {
            string[] linesArray = ReadAllLines(yaml);
            List<string> lines = new List<string>(linesArray);
            int xiaobaiIndex = Array.FindIndex(linesArray, l => l.Trim() == "xiaobai:");
            if (xiaobaiIndex == -1)
            {
                MessageBox.Show("未能找到 xiaobai 配置。");
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("shadow_radius:"))
                {
                    lines[i] = "    shadow_radius: 0";
                }
                if (lines[i].Contains("shadow_offset_x:"))
                {
                    lines[i] = "    shadow_offset_x: 4";
                }
                if (lines[i].Contains("shadow_offset_y:"))
                {
                    lines[i] = "    shadow_offset_y: 12";
                }
            }

            lines[xiaobaiIndex + 1] = "    name: 小白输入法／xiaobai";
            lines[xiaobaiIndex + 2] = "    text_color: 0xFF000000";
            lines[xiaobaiIndex + 3] = "    back_color: 0xFFeceeee";
            lines[xiaobaiIndex + 4] = "    shadow_color: 0xFF000000";
            lines[xiaobaiIndex + 5] = "    border_color: 0xeFF0e0e0";
            lines[xiaobaiIndex + 6] = "    hilited_text_color: 0xFF000000";
            lines[xiaobaiIndex + 7] = "    hilited_back_color: 0xFFd4d4d4";
            lines[xiaobaiIndex + 8] = "    hilited_shadow_color: 0xFF000000";
            lines[xiaobaiIndex + 9] = "    hilited_candidate_text_color: 0xFFffffff";
            lines[xiaobaiIndex + 10] = "    hilited_candidate_back_color: 0xFFfa3a0a";
            lines[xiaobaiIndex + 11] = "    hilited_candidate_shadow_color: 0xFF000000";
            lines[xiaobaiIndex + 12] = "    candidate_text_color: 0xFF000000";
            lines[xiaobaiIndex + 13] = "    candidate_back_color: 0xFFeceeee";
            lines[xiaobaiIndex + 14] = "    candidate_shadow_color: 0xFF000000";

            File.WriteAllLines(yaml, lines.ToArray());
            RestartProgress();
            Application.ExitThread();
            Application.Exit();
            Application.Restart();
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            List<string> lines = new List<string>(ReadAllLines(yaml));
            string pattern = @"shadow_radius: \d+";
            for (int i = 0; i < lines.Count; i++)
            {
                if (Regex.IsMatch(lines[i], pattern))
                {
                    lines[i] = Regex.Replace(lines[i], pattern, "shadow_radius: " + trackBar1.Value);
                }
            }
            File.WriteAllLines(yaml, lines.ToArray());
            RestartProgress();
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            List<string> lines = new List<string>(ReadAllLines(yaml));
            string pattern = @"shadow_offset_x: \d+";
            for (int i = 0; i < lines.Count; i++)
            {
                if (Regex.IsMatch(lines[i], pattern))
                {
                    lines[i] = Regex.Replace(lines[i], pattern, "shadow_offset_x: " + trackBar2.Value);
                }
            }
            File.WriteAllLines(yaml, lines.ToArray());
            RestartProgress();
        }

        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            List<string> lines = new List<string>(ReadAllLines(yaml));
            string pattern = @"shadow_offset_y: \d+";
            for (int i = 0; i < lines.Count; i++)
            {
                if (Regex.IsMatch(lines[i], pattern))
                {
                    lines[i] = Regex.Replace(lines[i], pattern, "shadow_offset_y: " + trackBar3.Value);
                }
            }
            File.WriteAllLines(yaml, lines.ToArray());
            RestartProgress();
        }
    }
}