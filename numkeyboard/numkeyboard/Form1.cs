using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NumKeyboardTray
{
    public partial class Form1 : Form
    {
        private bool forceEnabled = false;

        private static IntPtr _hookID = IntPtr.Zero;
        private static LowLevelKeyboardProc _proc = HookCallback;

        private static DateTime lastNumPad0Time = DateTime.MinValue;
        private static int repeatCount = 0;
        private static bool isCtrlPressed = false;

        private Timer monitorTimer;
        private NotifyIcon notifyIcon;
        private static bool isAltPressed = false;
        private static Timer altReleaseTimer;

        // 配置文件路径
        private static readonly string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        private static readonly string configFile = Path.Combine(dataFolder, "numkeyboard.txt");

        // WinAPI 常量
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public Form1()
        {
            InitializeComponent();
            InitializeNotifyIcon();
            this.Hide();
            this.Text = "小白T9键盘驱动";
        }

        #region 接收 Program.cs 发来的消息
        protected override void WndProc(ref Message m)
        {
            const int WM_SHOWME = 0x8001;
            if (m.Msg == WM_SHOWME)
            {
                RestoreWindow();
            }
            base.WndProc(ref m);
        }
        #endregion

        public void RestoreWindow()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.Show();
            this.BringToFront();
            this.Activate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
            altReleaseTimer = new Timer();

            label6.MouseHover += Label6_MouseHover;
            label6.MouseLeave += Label6_MouseLeave;

            // 绑定 checkBox 控制开机自启
            checkBox1.Checked = IsAutoRunEnabled();
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;

            // 初始化下拉框
            string[] items = { "None", "ESC", "Shift", "Tab", "Ctrl", "0", "复制", "粘贴", "剪切", "全选", "空格", "打开我的电脑", "打开计算器", "打开浏览器主页", "打开邮件", "切换输入法" };
            comboBoxBrowserHome.Items.AddRange(items);
            comboBoxTab.Items.AddRange(items);
            comboBoxMail.Items.AddRange(items);
            comboBoxApp2.Items.AddRange(items);
            comboBoxCtrl.Items.AddRange(items);
            comboBox0.Items.AddRange(items);

            comboBoxBrowserHome.SelectedItem = LoadKeyMapping("BrowserHomeKey");
            comboBoxTab.SelectedItem = LoadKeyMapping("TabKey");
            comboBoxMail.SelectedItem = LoadKeyMapping("LaunchMailKey");
            comboBoxApp2.SelectedItem = LoadKeyMapping("LaunchApp2Key");
            comboBoxCtrl.SelectedItem = LoadKeyMapping("Single0Key");
            comboBox0.SelectedItem = LoadKeyMapping("Triple0Key");

            comboBoxBrowserHome.SelectedIndexChanged += (s, e2) =>
                SaveKeyMapping("BrowserHomeKey", comboBoxBrowserHome.SelectedItem.ToString());
            comboBoxTab.SelectedIndexChanged += (s, e2) =>
                SaveKeyMapping("TabKey", comboBoxTab.SelectedItem.ToString());
            comboBoxMail.SelectedIndexChanged += (s, e2) =>
                SaveKeyMapping("LaunchMailKey", comboBoxMail.SelectedItem.ToString());
            comboBoxApp2.SelectedIndexChanged += (s, e2) =>
                SaveKeyMapping("LaunchApp2Key", comboBoxApp2.SelectedItem.ToString());
            comboBoxCtrl.SelectedIndexChanged += (s, e2) =>
                SaveKeyMapping("Single0Key", comboBoxCtrl.SelectedItem.ToString());
            comboBox0.SelectedIndexChanged += (s, e2) =>
                SaveKeyMapping("Triple0Key", comboBox0.SelectedItem.ToString());

            // 热插拔检测
            monitorTimer = new Timer { Interval = 3000 };
            monitorTimer.Tick += (s, ev) => CheckKeyboardAndHook();
            monitorTimer.Start();

            CheckKeyboardAndHook();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon.ShowBalloonTip(1000, "提示", "程序已最小化到托盘", ToolTipIcon.Info);
                return;
            }

            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            monitorTimer?.Stop();

            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
        }

        #region 托盘和气泡提示
        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = numkeyboard.Properties.Resources.xiaobait9,
                Visible = true,
                Text = "小白T9键盘驱动 - 未检测到键盘"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("打开设置", null, (s, e) => RestoreWindow());
            menu.Items.Add("退出", null, (s, e) => Application.Exit());
            notifyIcon.ContextMenuStrip = menu;
            notifyIcon.DoubleClick += (s, e) => RestoreWindow();
        }

        private void ShowStatus(string status)
        {
            if (notifyIcon != null)
            {
                notifyIcon.Text = $"{status}";
                notifyIcon.BalloonTipTitle = "小白T9键盘驱动";
                notifyIcon.BalloonTipText = status;
                notifyIcon.ShowBalloonTip(2000);
            }
        }
        #endregion

        #region 开机自启
        private bool IsAutoRunEnabled()
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", false))
            {
                if (key == null) return false;
                return key.GetValue("NumKeyboardTray") != null;
            }
        }

        private void SetAutoRun(bool enable)
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                    key.SetValue("NumKeyboardTray", Application.ExecutablePath);
                else
                    key.DeleteValue("NumKeyboardTray", false);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SetAutoRun(checkBox1.Checked);
            ShowStatus(checkBox1.Checked ? "已设置开机自启" : "已取消开机自启");
        }
        #endregion

        #region 小键盘检测（热插拔）
        private void CheckKeyboardAndHook()
        {
            if (IsKeyboardInsertedByVIDPID())
            {
                if (_hookID == IntPtr.Zero)
                {
                    _hookID = SetHook(_proc);
                    ShowStatus("小白T9键盘已插入，键盘配置生效！");
                }
            }
            else
            {
                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                    ShowStatus("未检测到小白T9键盘，键盘配置关闭！");
                }
            }
        }

        private bool IsKeyboardInsertedByVIDPID()
        {
            string vid = "32C2";
            string pid = "0012";

            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
                foreach (ManagementObject device in searcher.Get())
                {
                    string id = device["PNPDeviceID"]?.ToString() ?? "";
                    if (id.Contains("VID_" + vid) && id.Contains("PID_" + pid))
                        return true;
                }
            }
            catch { }
            return false;
        }
        #endregion

        #region 键盘钩子
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kbData = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                uint vkCode = kbData.vkCode;

                // NumPad0 处理逻辑
                if (vkCode == (int)Keys.NumPad0)
                {
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        var now = DateTime.Now;

                        if ((now - lastNumPad0Time).TotalMilliseconds < 150)
                            repeatCount++;
                        else
                            repeatCount = 1;

                        lastNumPad0Time = now;

                        // 如果是3次连续按键（000）
                        if (repeatCount == 3)
                        {
                            HandleCustomKeyByType("Triple0Key");

                            if (isCtrlPressed)
                            {
                                keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                                isCtrlPressed = false;
                            }
                            repeatCount = 0;
                            return (IntPtr)1;
                        }
                        // 如果是第一次按键
                        else if (repeatCount == 1)
                        {
                            string single0Action = GetKeyMapping("Single0Key");
                            if (single0Action == "Ctrl")
                            {
                                keybd_event((byte)Keys.ControlKey, 0, 0, IntPtr.Zero);
                                isCtrlPressed = true;
                            }
                        }

                        return (IntPtr)1;
                    }
                    else if (wParam == (IntPtr)WM_KEYUP)
                    {
                        if (repeatCount < 3)
                        {
                            string single0Action = GetKeyMapping("Single0Key");

                            if (single0Action == "Ctrl" && isCtrlPressed)
                            {
                                keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                                isCtrlPressed = false;
                            }
                            else if (single0Action != "Ctrl" && single0Action != "None" && repeatCount == 1)
                            {
                                HandleCustomKeyByType("Single0Key");
                            }
                        }

                        var now = DateTime.Now;
                        if ((now - lastNumPad0Time).TotalMilliseconds >= 150)
                        {
                            repeatCount = 0;
                        }

                        return (IntPtr)1;
                    }
                }

                // 其他自定义键处理
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    switch (vkCode)
                    {
                        case 0xAC: // BrowserHome
                            HandleCustomKeyByType("BrowserHomeKey");
                            return (IntPtr)1;
                        case (int)Keys.Tab:
                            HandleCustomKeyByType("TabKey");
                            return (IntPtr)1;
                        case 0xB4: // LaunchMail
                            HandleCustomKeyByType("LaunchMailKey");
                            return (IntPtr)1;
                        case 0xB7: // LaunchApp2
                            HandleCustomKeyByType("LaunchApp2Key");
                            return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        #endregion

        #region 键位映射处理
        private void SaveKeyMapping(string keyName, string value)
        {
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            var lines = new List<string>();
            if (File.Exists(configFile))
                lines.AddRange(File.ReadAllLines(configFile));

            bool found = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(keyName + "="))
                {
                    lines[i] = keyName + "=" + value;
                    found = true;
                    break;
                }
            }
            if (!found)
                lines.Add(keyName + "=" + value);

            File.WriteAllLines(configFile, lines);
        }

        private string LoadKeyMapping(string keyName)
        {
            if (!File.Exists(configFile))
                return "None";

            var lines = File.ReadAllLines(configFile);
            foreach (var line in lines)
            {
                if (line.StartsWith(keyName + "="))
                    return line.Substring((keyName + "=").Length);
            }
            return "None";
        }

        private static string GetKeyMapping(string keyName)
        {
            string value = "None";
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            string configFile = Path.Combine(dataFolder, "numkeyboard.txt");
            if (File.Exists(configFile))
            {
                var lines = File.ReadAllLines(configFile);
                foreach (var line in lines)
                {
                    if (line.StartsWith(keyName + "="))
                    {
                        value = line.Substring((keyName + "=").Length);
                        break;
                    }
                }
            }
            return value;
        }

        private static void HandleCustomKeyByType(string keyName)
        {
            string value = GetKeyMapping(keyName);

            // 临时取消钩子，避免模拟按键被再次拦截
            bool hookWasActive = _hookID != IntPtr.Zero;
            if (hookWasActive)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            switch (value)
            {
                case "None":
                    break;
                case "打开我的电脑":
                    keybd_event(182, 0, 0, IntPtr.Zero);
                    keybd_event(182, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "打开计算器":
                    keybd_event(183, 0, 0, IntPtr.Zero);
                    keybd_event(183, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "打开浏览器主页":
                    keybd_event(172, 0, 0, IntPtr.Zero);
                    keybd_event(172, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "打开邮件":
                    keybd_event(180, 0, 0, IntPtr.Zero);
                    keybd_event(180, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "切换输入法":
                    keybd_event((byte)Keys.ControlKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.ShiftKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.ShiftKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "复制":
                    keybd_event((byte)Keys.ControlKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.C, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.C, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "粘贴":
                    keybd_event((byte)Keys.ControlKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.V, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.V, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "剪切":
                    keybd_event((byte)Keys.ControlKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.X, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.X, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "全选":
                    keybd_event((byte)Keys.ControlKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.A, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.A, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "ESC":
                    keybd_event((byte)Keys.Escape, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.Escape, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "空格":
                    keybd_event((byte)Keys.Space, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.Space, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "Shift":
                    keybd_event((byte)Keys.ShiftKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.ShiftKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "Tab":
                    keybd_event((byte)Keys.Tab, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.Tab, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "Ctrl":
                    keybd_event((byte)Keys.ControlKey, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.ControlKey, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
                case "0":
                    keybd_event((byte)Keys.NumPad0, 0, 0, IntPtr.Zero);
                    keybd_event((byte)Keys.NumPad0, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                    break;
            }

            // 重新设置钩子
            if (hookWasActive)
            {
                _hookID = SetHook(_proc);
            }
        }
        #endregion

        #region 图片提示
        private ImageTooltipForm tooltipForm;

        private void Label6_MouseHover(object sender, EventArgs e)
        {
            var img = numkeyboard.Properties.Resources.numkeyboard;
            tooltipForm = new ImageTooltipForm(img);
            Point pos = Cursor.Position;
            tooltipForm.Location = new Point(pos.X + 10, pos.Y + 10);
            tooltipForm.Show();
        }

        private void Label6_MouseLeave(object sender, EventArgs e)
        {
            if (tooltipForm != null)
            {
                tooltipForm.Close();
                tooltipForm = null;
            }
        }
        #endregion

        #region WinAPI
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);
        #endregion

        private void label6_Click(object sender, EventArgs e)
        {
            Process.Start("https://t9.xiaobai.pro/?p=508");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 切换强制模式
            forceEnabled = !forceEnabled;

            if (forceEnabled)
            {
                // 停止热插拔检测
                monitorTimer.Stop();

                // 若钩子未激活，则强制安装
                if (_hookID == IntPtr.Zero)
                {
                    _hookID = SetHook(_proc);
                }

                ShowStatus("【强制模式已开启】键盘无需插入，改键已启用");
                button1.Text = "关闭强制模式";
            }
            else
            {
                // 关闭强制模式
                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }

                // 恢复自动热插拔检测
                monitorTimer.Start();

                ShowStatus("【强制模式已关闭】已恢复键盘识别模式");
                button1.Text = "强制开启改键";
            }
        }

    }
}
