using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace XiaobaiT9Updater
{
    public partial class UpdateForm : Form
    {
        private readonly string installPath;
        private readonly string currentVersion;
        private readonly string configFile;
        private readonly string updateApiUrl = "https://t9.xiaobai.pro/update.json";

        // 全部改成实例变量，解决“第二次点击无效”问题
        private bool updatePromptShown = false;
        private bool updateChecked = false;
        private bool isCheckingForUpdate = false;

        public UpdateForm()
        {
            InitializeComponent();

            installPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            Debug.WriteLine("当前安装路径: " + installPath);

            currentVersion = GetCurrentVersionFromPath(installPath);
            configFile = Path.Combine(installPath, "update_config.json");

            textBox3.Text = "安装路径:\r\n" + installPath + "\r\n\r\n当前版本: " + currentVersion;
            chkAutoUpdateOnStartup.Checked = LoadAutoCheckSetting();
            lblVersion.Text = "当前版本: " + currentVersion;

            // 事件绑定
            this.Load += UpdateForm_Load;
            btnCheckUpdate.Click += btnCheckUpdate_Click;
            chkAutoUpdateOnStartup.CheckedChanged += chkAutoCheck_CheckedChanged;

            // 新增：强制下载按钮（你拖的控件名必须是 btnForceDownload）
            if (btnForceDownload != null)
                btnForceDownload.Click += btnForceDownload_Click;
        }

        private string GetCurrentVersionFromPath(string path)
        {
            try
            {
                string folderName = Path.GetFileName(path.TrimEnd('\\'));
                Debug.WriteLine("文件夹名: " + folderName);

                if (folderName.StartsWith("xiaobait9-", StringComparison.OrdinalIgnoreCase))
                {
                    string versionPart = folderName.Substring("xiaobait9-".Length);
                    if (DateTime.TryParseExact(versionPart, "yyyy.MM.dd", null,
                        System.Globalization.DateTimeStyles.None, out DateTime dt))
                    {
                        return dt.ToString("yyyy.MM.dd");
                    }
                }

                var parentDir = Directory.GetParent(path);
                if (parentDir != null)
                {
                    string parentName = parentDir.Name;
                    if (parentName.StartsWith("xiaobait9-", StringComparison.OrdinalIgnoreCase))
                    {
                        string versionPart = parentName.Substring("xiaobait9-".Length);
                        if (DateTime.TryParseExact(versionPart, "yyyy.MM.dd", null,
                            System.Globalization.DateTimeStyles.None, out DateTime dt))
                        {
                            return dt.ToString("yyyy.MM.dd");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("解析版本失败: " + ex.Message);
            }
            return "未知版本";
        }

        private async void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            if (isCheckingForUpdate) return;

            isCheckingForUpdate = true;
            UpdateButtonState();
            await CheckUpdateAsync();
            isCheckingForUpdate = false;
            UpdateButtonState();
        }

        private async void UpdateForm_Load(object sender, EventArgs e)
        {
            await Task.Delay(800);
            if (!isCheckingForUpdate)
            {
                isCheckingForUpdate = true;
                UpdateButtonState();
                await CheckUpdateAsync();
                isCheckingForUpdate = false;
                UpdateButtonState();
            }
        }

        private void UpdateButtonState()
        {
            this.Invoke((Action)(() =>
            {
                btnCheckUpdate.Enabled = !isCheckingForUpdate;
                btnCheckUpdate.Text = isCheckingForUpdate ? "正在检查更新…" : "检查更新";
            }));
        }

        // ==================== 强制下载最新版按钮事件 ====================
        private async void btnForceDownload_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要强制下载并安装最新版吗？\n\n这会覆盖当前版本，即使当前已是最新版。",
                "强制下载最新版", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                btnForceDownload.Enabled = false;
                btnForceDownload.Text = "正在获取最新版…";
                await ForceDownloadLatestAsync();
                btnForceDownload.Enabled = true;
                btnForceDownload.Text = "强制下载最新版";
            }
        }

        private async Task ForceDownloadLatestAsync()
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    var json = await client.GetStringAsync(updateApiUrl);
                    var info = JsonConvert.DeserializeObject<UpdateInfo>(json);

                    if (info == null || string.IsNullOrEmpty(info.download_url))
                    {
                        MessageBox.Show("无法获取最新版下载地址。", "错误");
                        return;
                    }

                    await DownloadAndRunUpdate(client, info.download_url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("强制下载失败：\r\n" + ex.Message, "错误");
            }
        }
        // ================================================================

        private async Task CheckUpdateAsync()
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    var json = await client.GetStringAsync(updateApiUrl);
                    var info = JsonConvert.DeserializeObject<UpdateInfo>(json);

                    if (info == null || string.IsNullOrEmpty(info.version))
                    {
                        MessageBox.Show("无法解析更新信息。", "错误");
                        return;
                    }

                    if (!DateTime.TryParseExact(currentVersion, "yyyy.MM.dd", null,
                        System.Globalization.DateTimeStyles.None, out DateTime localVersion))
                    {
                        MessageBox.Show("本地版本格式错误，无法比较。", "错误");
                        return;
                    }

                    if (!DateTime.TryParseExact(info.version, "yyyy.MM.dd", null,
                        System.Globalization.DateTimeStyles.None, out DateTime remoteVersion))
                    {
                        MessageBox.Show("服务器版本格式错误。", "错误");
                        return;
                    }

                    Debug.WriteLine($"本地版本: {localVersion:yyyy.MM.dd}");
                    Debug.WriteLine($"服务器版本: {remoteVersion:yyyy.MM.dd}");

                    // 每次检查都给出反馈（已修复第二次无效问题）
                    if (localVersion >= remoteVersion)
                    {
                        MessageBox.Show(
                            $"已经是最新版！\n\n当前版本：{localVersion:yyyy.MM.dd}\n官网最新版：{remoteVersion:yyyy.MM.dd}",
                            "提示",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    textBox2.Text = info.download_url;

                    if (!updatePromptShown)
                    {
                        updatePromptShown = true;
                        if (MessageBox.Show($"发现新版本：{info.version}\n\n当前版本：{currentVersion}\n是否立即下载并安装？",
                            "有新版本可用", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            await DownloadAndRunUpdate(client, info.download_url);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("检查更新失败：\r\n" + ex.Message, "错误");
            }
        }

        private async Task DownloadAndRunUpdate(HttpClient client, string url)
        {
            string temp = Path.Combine(Path.GetTempPath(), "xiaobai_t9_update.exe");
            textBox1.Text = temp;

            try
            {
                progressBarDownload.Value = 0;
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                long bytesRead = 0;

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        bytesRead += read;

                        if (totalBytes > 0)
                        {
                            int progress = (int)(bytesRead * 100 / totalBytes);
                            this.Invoke((Action)(() => progressBarDownload.Value = progress));
                        }
                    }
                }

                Process.Start(new ProcessStartInfo(temp) { UseShellExecute = true });
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("下载失败：\r\n" + ex.Message, "错误");
                if (File.Exists(temp)) File.Delete(temp);
            }
        }

        // ==================== 自动启动设置相关（不变） ====================
        private bool LoadAutoCheckSetting()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    var json = File.ReadAllText(configFile);
                    var cfg = JsonConvert.DeserializeObject<Config>(json);
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
            try
            {
                File.WriteAllText(configFile, JsonConvert.SerializeObject(cfg, Formatting.Indented));
            }
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
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key.SetValue("XiaobaiT9Updater", "\"" + Application.ExecutablePath + "\"");
                }
            }
            catch { }
        }

        private void RemoveFromStartup()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key.DeleteValue("XiaobaiT9Updater", false);
                }
            }
            catch { }
        }

        private void chkAutoCheck_CheckedChanged(object sender, EventArgs e)
        {
            SaveAutoCheckSetting();
        }
    }

    public class UpdateInfo
    {
        [JsonProperty("latest_version")]
        public string version { get; set; }
        public string download_url { get; set; }
    }

    public class Config
    {
        public bool autoCheck { get; set; } = true;
        public bool autoUpdateOnStartup { get; set; } = true;
    }
}