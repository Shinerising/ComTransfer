using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComTransfer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ComPort port;
        public MainWindow()
        {
            port = new ComPort();

            DataContext = port;

            InitializeComponent();

            CheckAutoRun();
        }

        private void CheckAutoRun()
        {
            if (ConfigurationManager.AppSettings["autorun"]?.ToUpper() == "TRUE")
            {
                port.DelayOpen(1000);
            }
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            if (port.IsOpen)
            {
                port.ClosePort();
            }
            else
            {
                port.OpenPort();
            }
        }

        private void Button_Option_Click(object sender, RoutedEventArgs e)
        {
            new ConfigWindow(this).ShowDialog();

            try
            {
                port.InitialPort();
            }
            catch
            {

            }
        }

        private void Button_Task_Click(object sender, RoutedEventArgs e)
        {
            new TaskWindow(this).ShowDialog();

            TaskManager.Instance.Refresh();
        }

        private void Button_Pull_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (port.IsReceiving && port.IsSending)
            {
                MessageBoxResult result = MessageBox.Show("拉取文件可能会中断当前的文件传输任务，是否确定拉取操作？", "操作提示", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            button.IsEnabled = false;

            port.SubmitCommand("fetch", port.PullFilePath);

            Task.Factory.StartNew(() => {
                Thread.Sleep(3000);
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    button.IsEnabled = true;
                }));
            });
        }

        private void Button_Push_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (port.IsReceiving && port.IsSending)
            {
                MessageBoxResult result = MessageBox.Show("发送文件可能会中断当前的文件传输任务，是否确定发送操作？", "操作提示", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            button.IsEnabled = false;

            port.SendFile(port.SelectedFilePath);

            Task.Factory.StartNew(() => {
                Thread.Sleep(3000);
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    button.IsEnabled = true;
                }));
            });
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "选择要发送的文件",
                DefaultExt = ".*",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "全部文件|*.*"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                port.SelectFile(dialog.FileName);
            }
        }
        private void Button_SelectRemote_Click(object sender, RoutedEventArgs e)
        {
            FileWindow window = new FileWindow(this);
            bool? result = window.ShowDialog();
            if (result == true)
            {
                port.SelectRemotePath(window.Path);
            }
        }
        private void Button_ClearRecord_Click(object sender, RoutedEventArgs e)
        {
            port.ClearRecord();
        }
        private void Button_ClearLog_Click(object sender, RoutedEventArgs e)
        {
            port.ClearLog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            port.Dispose();
        }

        public void SubmitCommand(string command, string param)
        {
            port.SubmitCommand(command, param);
        }

        public string LastCommand
        {
            get
            {
                return port.LastCommand;
            }
            set
            {
                port.LastCommand = value;
            }
        }
        private void OpenHelpFile()
        {
            string helpFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.html");

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(helpFile);
                    process.Start();
                }
            }
            catch
            {

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                OpenHelpFile();
            }
        }

        private void Label_Folder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;

            try
            {
                string folder = label.Content.ToString().Split('>')[1].TrimStart('[').TrimEnd(']');
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(folder);
                    process.Start();
                }
            }
            catch
            {

            }
        }
        private void Label_Com_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string command = "/K mode COM" + port.PortID;
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo("cmd.exe", command) { WorkingDirectory = "/" };
                    process.Start();
                }
            }
            catch
            {

            }
        }
    }
}
