using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace ComTransfer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 设置窗口显示模式（正常、最大化、最小化等）
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nCmdShow">窗口显示模式</param>
        /// <returns>设置结果</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 将窗口激活至前台
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>设置结果</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        /// <summary>
        /// 应用程序启动事件响应
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // 检查是否已启动的相同进程，若是则关闭当前待启动程序并激活原进程的主要窗口
            var processCurrent = Process.GetCurrentProcess();
            Process process = Process.GetProcessesByName(processCurrent.ProcessName).FirstOrDefault(item => item.Id != processCurrent.Id && item.MainWindowHandle != IntPtr.Zero);
            if (process != null)
            {
                ShowWindow(process.MainWindowHandle, 9);
                SetForegroundWindow(process.MainWindowHandle);
                Environment.Exit(0);
                return;
            }
        }
    }
}
