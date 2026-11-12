using System;
using System.IO;
using System.Windows.Forms;

namespace XiaobaiT9Updater
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // 以管理员身份完成语法模型的安装/删除（由主界面在权限不足时提权重启触发）
            if (args.Length >= 1 && (args[0] == "--apply-gram" || args[0] == "--remove-gram"))
            {
                string installPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
                try
                {
                    if (args[0] == "--apply-gram")
                    {
                        if (args.Length < 2 || !File.Exists(args[1]))
                            throw new FileNotFoundException("未找到下载好的模型文件。");
                        GramManager.Apply(args[1], installPath);
                    }
                    else
                    {
                        GramManager.Remove(installPath);
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "语法模型操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdateForm());
            return 0;
        }
    }
}
