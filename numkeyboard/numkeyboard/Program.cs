using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NumKeyboardTray
{
    internal static class Program
    {
        private static Mutex mutex;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_SHOWME = 0x8001; // 自定义消息

        [STAThread]
        static void Main()
        {
            bool createdNew;
            mutex = new Mutex(true, "NumKeyboardTray_Mutex", out createdNew);

            if (!createdNew)
            {
                // 已有实例 → 发送恢复窗口消息
                IntPtr hWnd = FindWindow(null, "小白T9键盘驱动");
                if (hWnd != IntPtr.Zero)
                {
                    PostMessage(hWnd, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
