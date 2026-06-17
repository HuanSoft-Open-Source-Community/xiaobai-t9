using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Security.Principal;
using System.Text;
using System.Drawing;
using System.Collections.Generic;


namespace XiaobaiT9Updater
{
    public partial class UpdateForm : Form
    {
        private readonly string installPath;
        private readonly string currentVersion;
        private readonly string configFile;
        private readonly string updateApiUrl = "https://t9.xiaobai.pro/update.json";
        private bool isCheckingForUpdate = false;
        private bool isStartupCheck = false;
        private bool cancelAutoExit = false;

        // 语法模型文件路径
        private string gramFilePath;
        // schema 文件路径（需要修改 grammar 配置的 yaml 文件）
        private string schemaFilePath;

        public UpdateForm()
        {
            InitializeComponent();
            installPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            currentVersion = GetCurrentVersionFromPath(installPath);
            configFile = Path.Combine(installPath, "update_config.json");

            // 新增：schema 文件路径
            schemaFilePath = Path.Combine(installPath, "data", "xiaobai_simp.schema.yaml");
            gramFilePath = Path.Combine(installPath, "data", "wanxiang-lts-zh-hans.gram");

            textBox3.Text = "安装路径:\r\n" + installPath + "\r\n\r\n当前版本: " + currentVersion;

            chkAutoUpdateOnStartup.Checked = LoadAutoCheckSetting();
            lblVersion.Text = "当前版本: " + currentVersion;

            if (lblLatestVersion != null)
                lblLatestVersion.Text = "最新版本: -";

            this.Load += UpdateForm_Load;
            btnCheckUpdate.Click += btnCheckUpdate_Click;
            chkAutoUpdateOnStartup.CheckedChanged += chkAutoUpdateOnStartup_CheckedChanged;
            if (btnForceDownload != null)
                btnForceDownload.Click += btnForceDownload_Click;
            //if (btnManageGram != null)
            //    btnManageGram.Click += btnManageGram_Click;
            // 【新增】：程序关闭时检查并确保算法服务运行
            this.FormClosing += UpdateForm_FormClosing;
        }

        private string GetCurrentVersionFromPath(string path)
        {
            try
            {
                string folder = Path.GetFileName(path.TrimEnd('\\'));
                if (folder.StartsWith("xiaobait9-", StringComparison.OrdinalIgnoreCase))
                {
                    string v = folder.Substring("xiaobait9-".Length);
                    if (DateTime.TryParseExact(v, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out _))
                        return v;
                }
                var parent = Directory.GetParent(path);
                if (parent != null && parent.Name.StartsWith("xiaobait9-", StringComparison.OrdinalIgnoreCase))
                {
                    string v = parent.Name.Substring("xiaobait9-".Length);
                    if (DateTime.TryParseExact(v, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out _))
                        return v;
                }
            }
            catch { }
            return "未知版本";
        }

        private bool IsRunningAsAdmin()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        private string GetFriendlyOSName()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string productName = key.GetValue("ProductName")?.ToString() ?? "";
                        string currentBuild = key.GetValue("CurrentBuild")?.ToString() ?? "";
                        int build = 0;
                        int.TryParse(currentBuild, out build);
                        if (productName.Contains("Windows 11")) return "Windows 11";
                        if (productName.Contains("Windows 10"))
                        {
                            if (build >= 22000) return "Windows 11";
                            return "Windows 10";
                        }
                    }
                }
                var ver = Environment.OSVersion.Version;
                if (ver.Major == 10) return "Windows 10";
                if (ver.Major == 6 && ver.Minor == 3) return "Windows 8.1";
                if (ver.Major == 6 && ver.Minor == 2) return "Windows 8";
                if (ver.Major == 6 && ver.Minor == 1) return "Windows 7";
            }
            catch { }
            return "Windows Unknown";
        }

        private async Task<(string province, string city)> GetLocationAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    string json = await client.GetStringAsync("http://ip-api.com/json/?lang=zh-CN");
                    dynamic result = JsonConvert.DeserializeObject(json);
                    if (result.status == "success")
                    {
                        string p = (string)result.regionName;
                        string c = (string)result.city;
                        if (!string.IsNullOrEmpty(p))
                            p = p.Replace("省", "").Replace("市", "").Replace("自治区", "");
                        if (!string.IsNullOrEmpty(c))
                            c = c.Replace("市", "");
                        return (p, c);
                    }
                }
            }
            catch { }
            return ("", "");
        }

        private async Task ReportAsync(string action = "check", bool hasIntent = true)
        {
            const string reportUrl = "https://t9.xiaobai.pro/report.php";
            try
            {
                var (province, city) = await GetLocationAsync();
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var data = new
                {
                    ip = "auto",
                    version = currentVersion,
                    action = action,
                    intent = hasIntent ? 1 : 0,
                    os = GetFriendlyOSName(),
                    is64 = Environment.Is64BitOperatingSystem ? 1 : 0,
                    dotnet = Environment.Version.ToString(),
                    admin = IsRunningAsAdmin() ? 1 : 0,
                    silent = isStartupCheck ? 1 : 0,
                    province = province,
                    city = city
                };
                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                await client.PostAsync(reportUrl, content);
            }
            catch { }
        }

        private async void UpdateForm_Load(object sender, EventArgs e)
        {
            UpdateGramButtonState();
            await Task.Delay(800);
            if (!isCheckingForUpdate)
            {
                isCheckingForUpdate = true;
                isStartupCheck = true;
                UpdateButtonState();
                await CheckUpdateAsync();
                isCheckingForUpdate = false;
                UpdateButtonState();
            }
        }

        private async void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            if (btnCheckUpdate.Text.Contains("点击取消") || btnCheckUpdate.Text.Contains("s) 内点击取消"))
            {
                cancelAutoExit = true;
                btnCheckUpdate.Text = "检查更新";
                textBox3.Text += "\r\n\r\n[操作] 已取消自动关闭。";
                return;
            }
            if (isCheckingForUpdate) return;
            isCheckingForUpdate = true;
            isStartupCheck = false;
            if (lblLatestVersion != null) lblLatestVersion.Text = "正在获取...";
            UpdateButtonState();
            await CheckUpdateAsync();
            isCheckingForUpdate = false;
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateButtonState));
                return;
            }
            if (!btnCheckUpdate.Text.Contains("点击取消"))
            {
                btnCheckUpdate.Enabled = !isCheckingForUpdate;
                btnCheckUpdate.Text = isCheckingForUpdate ? "正在检查更新…" : "检查更新";
            }
        }

        private async Task CheckUpdateAsync()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                string json = await client.GetStringAsync(updateApiUrl);
                UpdateInfo info = JsonConvert.DeserializeObject<UpdateInfo>(json);

                await ReportAsync("check", true);

                if (info == null || string.IsNullOrEmpty(info.version))
                {
                    if (!isStartupCheck)
                        MessageBox.Show("无法获取更新信息。", "提示");
                    return;
                }

                if (lblLatestVersion != null)
                    lblLatestVersion.Text = "最新版本: " + info.version;

                if (!DateTime.TryParseExact(currentVersion, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out DateTime local) ||
                    !DateTime.TryParseExact(info.version, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out DateTime remote))
                {
                    if (!isStartupCheck) MessageBox.Show("版本格式错误", "错误");
                    return;
                }

                // ==================== 已经是最新版本（恢复倒计时自动关闭） ====================
                if (local >= remote)
                {
                    if (!isStartupCheck)
                    {
                        // 手动检查时，只显示提示，不自动关闭
                        MessageBox.Show($"当前已是最新版本！\n\n当前版本：{currentVersion}\n最新版本：{info.version}",
                            "已是最新版", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        // 启动时检查 → 显示倒计时10秒后自动关闭
                        textBox3.Text = $"检测结果：已经是最新版！\r\n\r\n当前版本：{currentVersion}\r\n官网最新版：{info.version}\r\n\r\n程序即将自动退出... (点击任意按钮可取消)";

                        cancelAutoExit = false;
                        btnCheckUpdate.Enabled = true;

                        for (int i = 10; i > 0; i--)
                        {
                            if (cancelAutoExit) return;
                            btnCheckUpdate.Text = $"已是最新版！！ ({i}s) 内点击取消自动关闭";
                            await Task.Delay(1000);
                        }

                        if (!cancelAutoExit)
                            Application.Exit();
                        else
                            btnCheckUpdate.Text = "检查更新";

                        return;
                    }
                }

                // ==================== 有新版本 → 三个选项 ====================
                DialogResult result = MessageBox.Show(
                    $"发现新版本：{info.version}\n\n" +
                    $"当前版本：{currentVersion}\n\n" +
                    $"请选择要使用的更新方式：\n\n" +
                    $"【是】 = 去官网下载最新安装包\n" +
                    $"【否】 = 打开微软商店更新\n" +
                    $"【取消】 = 不更新，关闭窗口",
                    "发现新版本 - 请选择更新方式",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Yes)
                {
                    await ReportAsync("official_download", true);
                    OpenOfficialWebsite();
                }
                else if (result == DialogResult.No)
                {
                    await ReportAsync("store_update", true);
                    OpenMicrosoftStorePage();
                }
                else
                {
                    await ReportAsync("cancel_update", false);
                }
            }
            catch (Exception ex)
            {
                if (!isStartupCheck)
                    MessageBox.Show("检查更新失败：\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== 两个辅助方法（必须添加） ====================

        private void OpenOfficialWebsite()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://t9.xiaobai.pro",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开官网失败：" + ex.Message, "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenMicrosoftStorePage()
        {
            try
            {
                string storeUrl = "ms-windows-store://pdp/?ProductId=XPDP2P7DFSHH7B";
                Process.Start(new ProcessStartInfo
                {
                    FileName = storeUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                // 备用网页版
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://apps.microsoft.com/detail/XPDP2P7DFSHH7B",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开微软商店失败：" + ex.Message, "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }




        private async void btnForceDownload_Click(object sender, EventArgs e)
        {
            string url = "https://t9.xiaobai.pro";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true   // 必须设置为 true，才能用默认浏览器打开
            };

            Process.Start(psi);
        }




        //private async void btnForceDownload_Click(object sender, EventArgs e)
        //{
        //    cancelAutoExit = true;
        //    if (btnCheckUpdate.Text.Contains("点击取消"))
        //        btnCheckUpdate.Text = "检查更新";

        //    if (MessageBox.Show("确定要强制下载并安装最新版吗？\n\n即使已是最新版也会重新覆盖。",
        //        "强制下载最新版", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        //        return;

        //    btnForceDownload.Enabled = false;
        //    btnForceDownload.Text = "正在获取…";
        //    if (lblLatestVersion != null) lblLatestVersion.Text = "正在获取...";

        //    try
        //    {
        //        HttpClient client = new HttpClient();
        //        client.Timeout = TimeSpan.FromSeconds(30);
        //        string json = await client.GetStringAsync(updateApiUrl);
        //        UpdateInfo info = JsonConvert.DeserializeObject<UpdateInfo>(json);
        //        await ReportAsync("force_update", true);

        //        if (info != null && !string.IsNullOrEmpty(info.version))
        //            if (lblLatestVersion != null) lblLatestVersion.Text = "最新版本: " + info.version;

        //        if (info != null && !string.IsNullOrEmpty(info.download_url))
        //            await DownloadAndRunUpdate(client, info.download_url);
        //        else
        //            MessageBox.Show("无法获取下载地址", "错误");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("强制下载失败：\n" + ex.Message, "错误");
        //        if (lblLatestVersion != null) lblLatestVersion.Text = "最新版本: 获取失败";
        //    }
        //    finally
        //    {
        //        btnForceDownload.Enabled = true;
        //        btnForceDownload.Text = "强制下载最新版";
        //    }
        //}

        //private async Task DownloadAndRunUpdate(HttpClient client, string url)
        //{
        //    string temp = Path.Combine(Path.GetTempPath(), "xiaobai_t9_update.exe");
        //    textBox1.Text = temp;
        //    try
        //    {
        //        progressBarDownload.Value = 0;
        //        HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        //        response.EnsureSuccessStatusCode();
        //        long? total = response.Content.Headers.ContentLength;
        //        long read = 0;
        //        Stream stream = await response.Content.ReadAsStreamAsync();
        //        FileStream fs = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None);
        //        try
        //        {
        //            byte[] buffer = new byte[8192];
        //            int bytes;
        //            while ((bytes = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //            {
        //                await fs.WriteAsync(buffer, 0, bytes);
        //                read += bytes;
        //                if (total.HasValue && total.Value > 0)
        //                {
        //                    int p = (int)(read * 100 / total.Value);
        //                    if (InvokeRequired)
        //                        Invoke(new Action(() => progressBarDownload.Value = p));
        //                    else
        //                        progressBarDownload.Value = p;
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            fs.Close();
        //            stream.Close();
        //        }

        //        Process.Start(new ProcessStartInfo(temp) { UseShellExecute = true });
        //        Application.Exit();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("下载失败：\n" + ex.Message, "错误");
        //        if (File.Exists(temp)) File.Delete(temp);
        //    }
        //}

        ////private async void btnManageGram_Click(object sender, EventArgs e)
        //{
        //    // 新快捷方式：启动算法服务（相当于重启小狼毫）
        //    string serviceShortcutPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\小白T9输入法\小白T9输入法算法服务.lnk";

        //    // 如果即将下载模型，也取消自动退出倒计时
        //    if (!File.Exists(gramFilePath))
        //    {
        //        cancelAutoExit = true;
        //        if (btnCheckUpdate.Text.Contains("点击取消") || btnCheckUpdate.Text.Contains("s) 内点击取消"))
        //        {
        //            btnCheckUpdate.Text = "检查更新";
        //            textBox3.Text += "\r\n\r\n[操作] 已取消自动关闭（正在处理语法模型）。";
        //        }
        //    }

        //    btnManageGram.Enabled = false;

        //    try
        //    {
        //        // 第一步：强制停止算法服务（避免文件占用）
        //        btnManageGram.Text = "正在停止算法服务…";

        //        string tempBat = Path.Combine(Path.GetTempPath(), "xiaobai_t9_stop_service.bat");
        //        string batContent = "@echo off\ntaskkill /F /IM WeaselServer.exe >nul 2>&1\nping 127.0.0.1 -n 3 >nul\ndel /F /Q \"%~f0\"";

        //        File.WriteAllText(tempBat, batContent, Encoding.Default);

        //        ProcessStartInfo psiStop = new ProcessStartInfo(tempBat)
        //        {
        //            UseShellExecute = true,
        //            Verb = "runas",                 // 提升权限
        //            WindowStyle = ProcessWindowStyle.Hidden,
        //            CreateNoWindow = true
        //        };

        //        Process.Start(psiStop)?.WaitForExit(8000); // 等待最多8秒
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("停止算法服务失败：\n" + ex.Message + "\n\n将继续尝试后续操作，但可能失败。", "警告");
        //    }

        //    try
        //    {
        //        if (File.Exists(gramFilePath))
        //        {
        //            // ============ 删除模型 ============
        //            btnManageGram.Text = "正在删除模型文件…";

        //            // 直接删除（服务已停，应该不会被占用）
        //            File.Delete(gramFilePath);

        //            // 禁用 grammar 配置
        //            ToggleGrammarConfig(enable: false);

        //            MessageBox.Show("万象语法模型已成功删除。\n\n已自动禁用相关配置，正在重启算法服务以生效...", "操作成功");
        //        }
        //        else
        //        {
        //            // ============ 下载模型 ============
        //            btnManageGram.Text = "正在下载模型…";

        //            string dataFolder = Path.GetDirectoryName(gramFilePath);
        //            Directory.CreateDirectory(dataFolder);

        //            await DownloadGramFile();

        //            // 启用 grammar 配置（含缩进修正）
        //            ToggleGrammarConfig(enable: true);

        //            MessageBox.Show("万象语法模型下载完成。\n\n已自动启用相关配置，正在重启算法服务以生效...", "操作成功");
        //        }

        //        // 最后一步：启动算法服务快捷方式（重启小狼毫服务，加载新配置）
        //        if (File.Exists(serviceShortcutPath))
        //        {
        //            btnManageGram.Text = "正在启动算法服务…";
        //            await Task.Delay(500); // 给用户一点视觉反馈
        //            Process.Start(new ProcessStartInfo(serviceShortcutPath) { UseShellExecute = true });
        //        }
        //        else
        //        {
        //            MessageBox.Show("警告：未找到“小白T9输入法算法服务”快捷方式。\n\n请手动启动算法服务以使更改生效。", "提示");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("操作失败：\n" + ex.Message + "\n\n可能原因：权限不足、网络问题或文件被占用。", "错误");
        //    }
        //    finally
        //    {
        //        btnManageGram.Enabled = true;
        //        UpdateGramButtonState(); // 刷新按钮文字（下载/删除）
        //    }
        //}

        private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string serviceShortcutPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\小白T9输入法\小白T9输入法算法服务.lnk";

            try
            {
                // 检查 WeaselServer.exe 是否正在运行
                Process[] processes = Process.GetProcessesByName("WeaselServer");
                if (processes.Length == 0)
                {
                    // 服务未运行，且快捷方式存在 → 自动启动
                    if (File.Exists(serviceShortcutPath))
                    {
                        Process.Start(new ProcessStartInfo(serviceShortcutPath)
                        {
                            UseShellExecute = true
                        });
                    }
                    // 如果快捷方式不存在，不弹出提示（避免干扰用户正常关闭）
                }
                // 如果进程存在，什么都不做
            }
            catch
            {
                // 静默失败，不影响程序退出
            }
        }

















        //private async Task DownloadGramFile()
        //{
        //    string url = "https://github.com/amzxyz/RIME-LMDG/releases/download/LTS/wanxiang-lts-zh-hans.gram";
        //    HttpClient client = new HttpClient();
        //    client.Timeout = TimeSpan.FromSeconds(120); // 大文件建议超时长一点

        //    progressBarDownload.Value = 0;
        //    HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        //    response.EnsureSuccessStatusCode();

        //    long? total = response.Content.Headers.ContentLength;
        //    long read = 0;
        //    Stream stream = await response.Content.ReadAsStreamAsync();

        //    using (FileStream fs = new FileStream(gramFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        //    {
        //        byte[] buffer = new byte[8192];
        //        int bytes;
        //        while ((bytes = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //        {
        //            await fs.WriteAsync(buffer, 0, bytes);
        //            read += bytes;
        //            if (total.HasValue && total.Value > 0)
        //            {
        //                int p = (int)(read * 100 / total.Value);
        //                if (InvokeRequired)
        //                    Invoke(new Action(() => progressBarDownload.Value = p));
        //                else
        //                    progressBarDownload.Value = p;
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 启用或禁用 xiaobai_simp.schema.yaml 中的 grammar 配置块
        /// 精确识别从 "# grammar:" 开始的整个缩进块
        /// </summary>
        /// <param name="enable">true=取消注释（启用），false=加上注释（禁用）</param>
        private void ToggleGrammarConfig(bool enable)
        {
            if (!File.Exists(schemaFilePath))
            {
                MessageBox.Show("未找到 schema 文件：\n" + schemaFilePath, "警告");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(schemaFilePath, Encoding.UTF8);
                List<string> newLines = new List<string>();

                bool inGrammarBlock = false;

                foreach (string line in lines)
                {
                    string originalTrimmed = line.TrimStart(' ');
                    int leadingSpaceCount = line.Length - originalTrimmed.Length;

                    // 第一步：如果当前行是下一个顶级项，强制结束 grammar 块
                    if (inGrammarBlock &&
                        (originalTrimmed.StartsWith("speller:", StringComparison.Ordinal) ||
                         originalTrimmed.StartsWith("translator:", StringComparison.Ordinal) ||
                         originalTrimmed.StartsWith("key_binder:", StringComparison.Ordinal) ||
                         originalTrimmed.StartsWith("punctuator:", StringComparison.Ordinal) ||
                         originalTrimmed.StartsWith("recognizer:", StringComparison.Ordinal) ||
                         originalTrimmed.StartsWith("simplifier:", StringComparison.Ordinal)))
                    {
                        inGrammarBlock = false;
                    }

                    // 第二步：检测是否进入 grammar 块
                    if (!inGrammarBlock &&
                        (originalTrimmed.StartsWith("grammar:", StringComparison.Ordinal) ||
                         originalTrimmed.StartsWith("# grammar:", StringComparison.Ordinal) ||
                         originalTrimmed.StartsWith("#grammar:", StringComparison.Ordinal)))  // 兼容无空格情况
                    {
                        inGrammarBlock = true;
                    }

                    // 第三步：处理 grammar 块内的行
                    if (inGrammarBlock)
                    {
                        if (enable)
                        {
                            // ============ 启用：彻底去掉所有 # 注释 ============
                            string cleanContent;

                            if (originalTrimmed.StartsWith("#"))
                            {
                                // 去掉开头的 # 和它后面的所有空格
                                cleanContent = originalTrimmed.Substring(1).TrimStart(' ');
                            }
                            else
                            {
                                cleanContent = originalTrimmed;
                            }

                            // 去除可能残留的 #（防御性）
                            if (cleanContent.StartsWith("#"))
                                cleanContent = cleanContent.TrimStart('#', ' ');

                            if (string.IsNullOrWhiteSpace(cleanContent))
                            {
                                newLines.Add("");
                            }
                            else if (cleanContent.StartsWith("grammar:"))
                            {
                                // grammar: 行：前面完全无空格（按你要求）
                                newLines.Add("grammar:");
                            }
                            else
                            {
                                // 子配置行：前面正好 2 个空格
                                newLines.Add("  " + cleanContent);
                            }
                        }
                        else
                        {
                            // ============ 禁用：确保每行都有 # ============
                            if (!string.IsNullOrWhiteSpace(originalTrimmed) && !originalTrimmed.StartsWith("#"))
                            {
                                newLines.Add(new string(' ', leadingSpaceCount) + "#" + originalTrimmed);
                            }
                            else
                            {
                                newLines.Add(line); // 已经有 # 或为空行
                            }
                        }
                    }
                    else
                    {
                        // 不在 grammar 块，直接原样输出
                        newLines.Add(line);
                    }
                }

                File.WriteAllLines(schemaFilePath, newLines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改 schema 配置失败：\n{ex.Message}\n\n可能原因：文件被占用或权限不足。", "错误");
            }
        }





        private void UpdateGramButtonState()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateGramButtonState));
                return;
            }

            bool exists = File.Exists(gramFilePath);

            if (btnManageGram != null)
            {
                btnManageGram.Text = exists ? "删除万象语法模型" : "下载万象语法模型";
            }

            if (lblGramStatus != null)
            {
                if (exists)
                {
                    lblGramStatus.Text = "万象语法模型: 已安装";
                    lblGramStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblGramStatus.Text = "万象语法模型: 未安装";
                    lblGramStatus.ForeColor = Color.Red;
                }
            }
        }

        private bool LoadAutoCheckSetting()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    string json = File.ReadAllText(configFile);
                    Config cfg = JsonConvert.DeserializeObject<Config>(json);
                    return cfg?.autoCheck ?? true;
                }
            }
            catch { }
            return true;
        }

        private void SaveAutoCheckSetting()
        {
            var cfg = new Config
            {
                autoCheck = chkAutoUpdateOnStartup.Checked,
                autoUpdateOnStartup = chkAutoUpdateOnStartup.Checked
            };
            try { File.WriteAllText(configFile, JsonConvert.SerializeObject(cfg, Formatting.Indented)); }
            catch { }

            if (chkAutoUpdateOnStartup.Checked)
                AddToStartup();
            else
                RemoveFromStartup();
        }

        private void AddToStartup()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                    key.SetValue("XiaobaiT9Updater", "\"" + Application.ExecutablePath + "\"");
            }
            catch { }
        }

        private void RemoveFromStartup()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                    key.DeleteValue("XiaobaiT9Updater", false);
            }
            catch { }
        }

        private void chkAutoUpdateOnStartup_CheckedChanged(object sender, EventArgs e)
        {
            SaveAutoCheckSetting();
        }

        private void btnForceDownload_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 使用 search 协议在微软商店内搜索“小白T9输入法”
                string query = Uri.EscapeDataString("小白T9输入法");
                string storeSearchUrl = $"ms-windows-store://search/?query={query}";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = storeSearchUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                // 如果协议打不开，打开网页版搜索作为备用
                try
                {
                    string webSearchUrl = "https://apps.microsoft.com/search?query=" + Uri.EscapeDataString("小白T9输入法");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = webSearchUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("搜索微软商店失败：" + ex.Message, "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }

    public class UpdateInfo
    {
        [JsonProperty("latest_version")]
        public string version { get; set; }
        public string download_url { get; set; }
        public string report_url { get; set; }
    }

    public class Config
    {
        public bool autoCheck { get; set; } = true;
        public bool autoUpdateOnStartup { get; set; } = true;
    }
}