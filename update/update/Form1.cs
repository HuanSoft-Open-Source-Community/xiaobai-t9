using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Security.Principal;
using System.Drawing;


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
        private bool gramBusy = false;

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
            gramFilePath = Path.Combine(installPath, "data", GramManager.GramFileName);

            textBox3.Text = "安装路径:\r\n" + installPath + "\r\n\r\n当前版本: " + currentVersion + "\r\n";

            chkAutoUpdateOnStartup.Checked = LoadAutoCheckSetting();
            lblVersion.Text = "当前版本: " + currentVersion;

            if (lblLatestVersion != null)
                lblLatestVersion.Text = "最新版本: -";

            this.Load += UpdateForm_Load;
            btnCheckUpdate.Click += btnCheckUpdate_Click;
            chkAutoUpdateOnStartup.CheckedChanged += chkAutoUpdateOnStartup_CheckedChanged;
            if (btnForceDownload != null)
                btnForceDownload.Click += btnForceDownload_Click;
            if (btnManageGram != null)
                btnManageGram.Click += btnManageGram_Click;
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
                btnCheckUpdate.Enabled = !isCheckingForUpdate && !gramBusy;
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

        private void btnForceDownload_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "https://t9.xiaobai.pro",
                UseShellExecute = true   // 必须设置为 true，才能用默认浏览器打开
            };

            Process.Start(psi);
        }

        // ==================== 万象语法模型 ====================

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
                return;
            }
            textBox3.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + "\r\n");
        }

        private void EnableGramUi(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(EnableGramUi), enabled);
                return;
            }
            btnManageGram.Enabled = enabled;
            btnCheckUpdate.Enabled = enabled && !isCheckingForUpdate;
            btnForceDownload.Enabled = enabled;
            button1.Enabled = enabled;
        }

        private void UpdateProgress(int percent, string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, string>(UpdateProgress), percent, text);
                return;
            }
            progressBarDownload.Value = Math.Max(0, Math.Min(100, percent));
            lblGramStatus.Text = text;
            lblGramStatus.ForeColor = SystemColors.ControlText;
        }

        private async void btnManageGram_Click(object sender, EventArgs e)
        {
            if (gramBusy) return;

            // 正在做模型相关操作时，取消“已是最新版”的自动退出倒计时
            cancelAutoExit = true;
            if (btnCheckUpdate.Text.Contains("点击取消") || btnCheckUpdate.Text.Contains("s) 内点击取消"))
                btnCheckUpdate.Text = "检查更新";

            gramBusy = true;
            EnableGramUi(false);
            try
            {
                if (File.Exists(gramFilePath))
                    await RemoveGramAsync();
                else
                    await InstallGramAsync();
            }
            catch (Exception ex)
            {
                Log("操作失败：" + ex.Message);
                MessageBox.Show(this, ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                gramBusy = false;
                EnableGramUi(true);
                progressBarDownload.Value = 0;
                UpdateGramButtonState();
            }
        }

        private async Task InstallGramAsync()
        {
            bool engine = GramManager.EnginePresent(installPath);
            bool capable = engine && GramManager.EngineSupportsGrammar(installPath);
            string tip;
            if (capable)
                tip = "将下载官方万象 LTS 语法模型（约 398 MB）到：\r\n" + gramFilePath +
                      "\r\n\r\n下载完成后自动启用 grammar 配置并重新部署，首次部署约需一分钟。";
            else if (!engine)
                tip = "当前目录下没有 rime.dll（看起来不是安装目录，例如 bin\\Debug），" +
                      "下载后不会立刻生效。\r\n\r\n正式安装的版本自带 Octagram，不需要额外组件。\r\n\r\n仍要下载吗？\r\n\r\n目标位置：" + gramFilePath;
            else
                tip = "当前引擎里没有 Octagram 组件，模型下载后不会参与组句。\r\n\r\n仍要下载吗？\r\n\r\n目标位置：" + gramFilePath;

            if (MessageBox.Show(this, tip, "下载万象语法模型", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.OK)
                return;

            string part = Path.Combine(Path.GetTempPath(), GramManager.GramFileName + ".part");
            Log("开始下载语法模型（支持断点续传，临时文件：" + part + "）");
            await DownloadGramFile(part);

            bool applied;
            try
            {
                Log("正在停止算法服务、落地模型并重新部署（约需一分钟）……");
                await Task.Run(() => GramManager.Apply(part, installPath));
                applied = true;
            }
            catch (UnauthorizedAccessException)
            {
                Log("当前权限不足，正在申请管理员权限完成安装……");
                applied = GramManager.RelaunchElevated("--apply-gram", part);
            }
            finally
            {
                try { if (File.Exists(part)) File.Delete(part); } catch { }
            }

            if (!applied)
                throw new Exception("未能完成安装。请右键“以管理员身份运行”本程序后重试。");

            Log("已安装并启用语法模型。");
            MessageBox.Show(this,
                "万象语法模型已安装并启用。\r\n\r\n首次重新部署约需一分钟，输入法会自动重载。",
                "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task RemoveGramAsync()
        {
            if (MessageBox.Show(this,
                    "确定要删除万象语法模型并关闭 grammar 配置吗？\r\n\r\n删除后长句候选改用引擎内置词频，文件可随时重新下载。",
                    "删除万象语法模型", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            bool removed;
            try
            {
                Log("正在停止算法服务、删除模型并重新部署……");
                await Task.Run(() => GramManager.Remove(installPath));
                removed = true;
            }
            catch (UnauthorizedAccessException)
            {
                Log("当前权限不足，正在申请管理员权限完成删除……");
                removed = GramManager.RelaunchElevated("--remove-gram");
            }

            if (!removed)
                throw new Exception("未能删除模型。请确认已退出输入法，或以管理员身份运行本程序后重试。");

            Log("已删除语法模型并关闭 grammar 配置。");
            MessageBox.Show(this, "语法模型已删除，grammar 配置已关闭。", "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task DownloadGramFile(string partPath)
        {
            long existing = 0;
            try
            {
                if (File.Exists(partPath)) existing = new FileInfo(partPath).Length;
            }
            catch { existing = 0; }

            if (existing >= GramManager.GramFileSize)
            {
                try { File.Delete(partPath); } catch { }
                existing = 0;
            }
            if (existing > 0)
                Log("检测到未完成的下载，从 " + (existing / 1048576.0).ToString("F1") + " MB 处继续。");

            Exception last = null;
            foreach (string url in GramManager.GramUrls)
            {
                try
                {
                    Log("下载源：" + url);
                    await DownloadFrom(url, partPath, existing);

                    Log("正在校验文件完整性……");
                    long size = new FileInfo(partPath).Length;
                    if (size != GramManager.GramFileSize)
                        throw new Exception("文件大小不符（" + size + " 字节，应为 " + GramManager.GramFileSize + " 字节）");

                    string sha = GramManager.Sha256OfFile(partPath);
                    if (!string.Equals(sha, GramManager.GramSha256, StringComparison.OrdinalIgnoreCase))
                        throw new Exception("SHA-256 不匹配（" + sha.Substring(0, 16) + "…）");

                    Log("校验通过：" + sha.Substring(0, 16) + "…");
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    Log("该源未通过：" + ex.Message);
                    // 内容有问题就删掉重下；纯网络中断则保留，方便下次续传
                    bool badContent = ex.Message.IndexOf("不符", StringComparison.Ordinal) >= 0 ||
                                      ex.Message.IndexOf("SHA-256", StringComparison.Ordinal) >= 0;
                    if (badContent)
                    {
                        try { if (File.Exists(partPath)) File.Delete(partPath); } catch { }
                        existing = 0;
                    }
                }
            }

            throw new Exception("所有下载地址均失败：" + (last == null ? "未知错误" : last.Message) +
                                "\r\n\r\n若本机无法访问 GitHub，可把同一份文件上传到 https://t9.xiaobai.pro/models/wanxiang-lts-zh-hans.gram 后再试。");
        }

        private async Task DownloadFrom(string url, string partPath, long existing)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(120);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("XiaobaiT9Updater/1.0");

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (existing > 0)
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existing, null);

                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    bool resumed = existing > 0 && response.StatusCode == HttpStatusCode.PartialContent;
                    if (existing > 0 && !resumed) existing = 0;
                    if (!resumed) response.EnsureSuccessStatusCode();

                    long total = existing + (response.Content.Headers.ContentLength ?? Math.Max(0, GramManager.GramFileSize - existing));

                    using (var input = await response.Content.ReadAsStreamAsync())
                    using (var output = new FileStream(partPath, resumed ? FileMode.Append : FileMode.Create,
                               FileAccess.Write, FileShare.None, 81920, true))
                    {
                        byte[] buffer = new byte[81920];
                        long read = existing;
                        long speedBase = existing;
                        var speedWatch = Stopwatch.StartNew();
                        DateTime lastReport = DateTime.MinValue;
                        int bytes;

                        while ((bytes = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, bytes);
                            read += bytes;

                            if ((DateTime.UtcNow - lastReport).TotalMilliseconds >= 300)
                            {
                                lastReport = DateTime.UtcNow;
                                int percent = total > 0 ? (int)(read * 100 / total) : 0;
                                double speed = speedWatch.Elapsed.TotalSeconds > 0.2
                                    ? (read - speedBase) / speedWatch.Elapsed.TotalSeconds
                                    : 0;
                                UpdateProgress(percent, string.Format(
                                    "正在下载语法模型 {0:F1}/{1:F1} MB（{2:F1} MB/s）",
                                    read / 1048576.0, total / 1048576.0, speed / 1048576.0));
                            }
                        }
                    }
                }
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
            bool engine = GramManager.EnginePresent(installPath);
            bool capable = engine && GramManager.EngineSupportsGrammar(installPath);

            if (btnManageGram != null)
                btnManageGram.Text = exists ? "删除万象语法模型" : "下载万象语法模型（约 398 MB）";

            if (lblGramStatus != null)
            {
                if (!engine)
                {
                    // 开发目录（bin\Debug）下没有 rime.dll，这不是"引擎不支持"
                    lblGramStatus.Text = "万象语法模型: 未安装（当前目录没有 rime.dll，安装后自动检测）";
                    lblGramStatus.ForeColor = Color.DimGray;
                }
                else if (exists)
                {
                    lblGramStatus.Text = capable
                        ? "万象语法模型: 已安装"
                        : "万象语法模型: 已安装（当前引擎不含 Octagram，不会生效）";
                    lblGramStatus.ForeColor = capable ? Color.Green : Color.DarkOrange;
                }
                else
                {
                    lblGramStatus.Text = capable
                        ? "万象语法模型: 未安装"
                        : "万象语法模型: 未安装（当前引擎不含 Octagram）";
                    lblGramStatus.ForeColor = capable ? Color.DimGray : Color.Red;
                }
            }
        }

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

    /// <summary>
    /// 万象语法模型（wanxiang-lts-zh-hans.gram，418,071,596 字节）的安装与卸载。
    /// 安装流程与九键 1.4 一致：停止服务 → 落地模型 → 开关 grammar 段 → 重新部署 → 启动服务。
    /// </summary>
    internal static class GramManager
    {
        public const string GramFileName = "wanxiang-lts-zh-hans.gram";
        public const long GramFileSize = 418071596L;

        // 与九键 1.4/2.3 随包模型同一份文件（万象 LTS，CC BY 4.0），下载后逐字节校验
        public const string GramSha256 = "3457B69F5FE0FC4F5D211C878BBFA2E2361B90639952C9845C8E31E9D047CDE6";

        public static readonly string[] GramUrls =
        {
            // 自家 NAS（wp.xiaobai.pro:1111，物理目录 \\192.168.50.99\Download\输入法\models）
            "https://wp.xiaobai.pro:1111/d/%E8%BE%93%E5%85%A5%E6%B3%95/models/wanxiang-lts-zh-hans.gram",
            // 阿里云 ECS 镜像
            "https://t9.xiaobai.pro/models/wanxiang-lts-zh-hans.gram",
            // 上游原始发布（兜底）
            "https://github.com/amzxyz/rime-wanxiang-data/releases/download/LTS/wanxiang-lts-zh-hans.gram"
        };

        private static bool? grammarCapable;
        private static bool? enginePresent;

        public static string GramPath(string installPath)
        {
            return Path.Combine(installPath, "data", GramFileName);
        }

        public static string SchemaPath(string installPath)
        {
            return Path.Combine(installPath, "data", "xiaobai_simp.schema.yaml");
        }

        /// <summary>安装目录里到底有没有 rime.dll（开发目录 bin\Debug 下没有）。</summary>
        public static bool EnginePresent(string installPath)
        {
            if (enginePresent.HasValue) return enginePresent.Value;
            bool found = false;
            string[] candidates = { "rime.dll", Path.Combine("Win32", "rime.dll") };
            foreach (string name in candidates)
            {
                try
                {
                    if (File.Exists(Path.Combine(installPath, name))) { found = true; break; }
                }
                catch { }
            }
            enginePresent = found;
            return found;
        }

        /// <summary>检测 rime.dll 是否带 Octagram（语法模型）组件。</summary>
        public static bool EngineSupportsGrammar(string installPath)
        {
            if (grammarCapable.HasValue) return grammarCapable.Value;

            bool found = false;
            string[] candidates = { "rime.dll", Path.Combine("Win32", "rime.dll") };
            foreach (string name in candidates)
            {
                try
                {
                    string path = Path.Combine(installPath, name);
                    if (!File.Exists(path)) continue;
                    byte[] data = File.ReadAllBytes(path);
                    string text = Encoding.ASCII.GetString(data);
                    if (text.IndexOf("octagram", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = true;
                        break;
                    }
                }
                catch { }
            }
            grammarCapable = found;
            return found;
        }

        public static string Sha256OfFile(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "");
        }

        public static void Apply(string sourceFile, string installPath)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("未找到下载的模型文件：" + sourceFile);

            string target = GramPath(installPath);
            Directory.CreateDirectory(Path.GetDirectoryName(target));

            StopServer(installPath);
            File.Copy(sourceFile, target, true);
            ToggleGrammar(SchemaPath(installPath), true);
            Deploy(installPath);
            StartServer(installPath);
        }

        public static void Remove(string installPath)
        {
            string target = GramPath(installPath);

            StopServer(installPath);
            if (File.Exists(target)) File.Delete(target);
            ToggleGrammar(SchemaPath(installPath), false);
            Deploy(installPath);
            StartServer(installPath);
        }

        public static void StopServer(string installPath)
        {
            string[] exes =
            {
                Path.Combine(installPath, "WeaselServer.exe"),
                Path.Combine(installPath, "Win32", "WeaselServer.exe")
            };
            foreach (string exe in exes)
            {
                if (!File.Exists(exe)) continue;
                try
                {
                    var info = new ProcessStartInfo(exe, "/quit") { UseShellExecute = false, CreateNoWindow = true };
                    using (var process = Process.Start(info))
                    {
                        if (process != null) process.WaitForExit(6000);
                    }
                }
                catch { }
            }

            foreach (var process in Process.GetProcessesByName("WeaselServer"))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(4000);
                }
                catch { }
            }
        }

        public static void StartServer(string installPath)
        {
            try
            {
                var info = new ProcessStartInfo("schtasks.exe", "/run /tn \"XiaoBaiT9\\WeaselServer\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(info))
                {
                    if (process != null) process.WaitForExit(8000);
                }
            }
            catch { }

            if (Process.GetProcessesByName("WeaselServer").Length == 0)
            {
                try
                {
                    string exe = Path.Combine(installPath, "WeaselServer.exe");
                    if (File.Exists(exe))
                        Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
                }
                catch { }
            }
        }

        public static void Deploy(string installPath)
        {
            string exe = Path.Combine(installPath, "WeaselDeployer.exe");
            if (!File.Exists(exe)) exe = Path.Combine(installPath, "Win32", "WeaselDeployer.exe");
            if (!File.Exists(exe)) throw new FileNotFoundException("未找到 WeaselDeployer.exe，无法重新部署。");

            var info = new ProcessStartInfo(exe, "/deploy")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(exe)
            };
            using (var process = Process.Start(info))
            {
                if (process == null) throw new Exception("启动部署程序失败。");
                process.WaitForExit(600000);
            }
        }

        /// <summary>
        /// 打开/关闭 schema 里的 grammar 段。只改注释，不动缩进与行尾，保持 UTF-8 无 BOM（与仓库文件一致）。
        /// </summary>
        public static void ToggleGrammar(string schemaPath, bool enable)
        {
            if (!File.Exists(schemaPath)) throw new FileNotFoundException("未找到方案文件：" + schemaPath);

            byte[] raw = File.ReadAllBytes(schemaPath);
            bool hasBom = raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF;
            string text = new UTF8Encoding(false).GetString(raw, hasBom ? 3 : 0, raw.Length - (hasBom ? 3 : 0));
            string newline = text.Contains("\r\n") ? "\r\n" : "\n";
            List<string> lines = new List<string>(text.Replace("\r\n", "\n").Split('\n'));

            int start = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].TrimStart();
                if (trimmed.StartsWith("# grammar:", StringComparison.Ordinal) ||
                    trimmed.StartsWith("#grammar:", StringComparison.Ordinal) ||
                    trimmed.StartsWith("grammar:", StringComparison.Ordinal))
                {
                    start = i;
                    break;
                }
            }
            if (start < 0) throw new Exception("方案文件里没有 grammar 段，未做修改。");

            int end = lines.Count;
            for (int i = start + 1; i < lines.Count; i++)
            {
                string trimmed = lines[i].TrimStart();
                int indent = lines[i].Length - trimmed.Length;
                if (trimmed.Length == 0) continue;
                if (indent == 0 && !trimmed.StartsWith("#"))
                {
                    end = i;
                    break;
                }
            }

            for (int i = start; i < end; i++)
            {
                string line = lines[i];
                string trimmed = line.TrimStart();
                int indent = line.Length - trimmed.Length;

                if (enable)
                {
                    if (!trimmed.StartsWith("#")) continue;
                    string content = trimmed.Substring(1).TrimStart(' ');
                    lines[i] = content.Length == 0 ? string.Empty : new string(' ', indent) + content;
                }
                else
                {
                    if (trimmed.Length == 0 || trimmed.StartsWith("#")) continue;
                    lines[i] = new string(' ', indent) + "#" + trimmed;
                }
            }

            // grammar: 必须是顶格键；历史上被缩进过的文件在这里纠正
            if (enable && lines[start].TrimStart().StartsWith("grammar:", StringComparison.Ordinal))
                lines[start] = "grammar:";

            File.WriteAllText(schemaPath, string.Join(newline, lines.ToArray()), new UTF8Encoding(hasBom));
        }

        /// <summary>以管理员身份重启自身执行模型操作，成功返回 true。</summary>
        public static bool RelaunchElevated(params string[] arguments)
        {
            var builder = new StringBuilder();
            foreach (string argument in arguments)
            {
                if (builder.Length > 0) builder.Append(' ');
                builder.Append('"').Append(argument).Append('"');
            }

            var info = new ProcessStartInfo(Application.ExecutablePath, builder.ToString())
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                using (var process = Process.Start(info))
                {
                    if (process == null) return false;
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch { return false; }
        }
    }
}
