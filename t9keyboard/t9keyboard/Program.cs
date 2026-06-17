using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace t9keyboard
{
    static class Program
    {
        // 全局唯一标识符，随便写，只要不和其他程序重复就行
        private const string SingleInstanceMutexName = "T9Keyboard_SingleInstance_Mutex_2025";
        private static Mutex _mutex;

        // Win32 API 用于激活已有窗口
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        private const int SW_RESTORE = 9; // 还原窗口

        [STAThread]
        static void Main()
        {
            // 单实例判断
            _mutex = new Mutex(true, SingleInstanceMutexName, out bool isFirstInstance);
            if (!isFirstInstance)
            {
                // 已经有实例运行 → 激活已有窗口并退出
                ActivateExistingWindow();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 创建主键盘窗体（先不显示）
            Form mainKeyboardForm = new Form2();  // ←← 请确认这里是你的主悬浮窗类名（如 keyboard 则改成 new keyboard()）

            // 创建设置窗体
            help settingForm = new help();

            // 关键：设置窗体加载完成后自动点击“确定”按钮
            settingForm.Load += (s, e) =>
            {
                // 用一个短延迟确保所有控件（包括 yes 按钮）都已初始化
                System.Windows.Forms.Timer autoClickTimer = new System.Windows.Forms.Timer();
                autoClickTimer.Interval = 100;
                autoClickTimer.Tick += (ts, te) =>
                {
                    autoClickTimer.Stop();
                    autoClickTimer.Dispose();

                    // 自动点击“确定”按钮
                    settingForm.yes_Click(settingForm.yes, EventArgs.Empty);

                    // 隐藏设置窗口
                    settingForm.Hide();

                    // 显示主键盘
                    mainKeyboardForm.Show();
                };
                autoClickTimer.Start();
            };

            // 如果你完全不想让设置窗口闪一下，可以加下面几行（可选）
            // settingForm.WindowState = FormWindowState.Minimized;
            // settingForm.ShowInTaskbar = false;
            // settingForm.Opacity = 0;

            // 先显示设置窗体（会触发 Load 事件 → 自动点确定 → 隐藏 → 显示主窗体）
            settingForm.Show();

            // 启动消息循环，以主键盘窗体为主
            Application.Run(mainKeyboardForm);

            // 程序结束时释放互斥体
            _mutex?.ReleaseMutex();
        }

        /// <summary>
        /// 激活已运行的窗口
        /// </summary>
        private static void ActivateExistingWindow()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                {
                    if (IsIconic(process.MainWindowHandle))
                        ShowWindow(process.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(process.MainWindowHandle);
                    return;
                }
            }
        }
    }
}