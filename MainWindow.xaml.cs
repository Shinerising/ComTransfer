using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

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
            port.SubmitCommand("fetch", port.PullFilePath);
        }

        private void Button_Push_Click(object sender, RoutedEventArgs e)
        {
            if (port.IsReceiving)
            {
                MessageBoxResult result = MessageBox.Show("发送文件会中断当前的文件接收任务，是否确定发送？", "操作提示", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }
            port.SendFile(port.SelectedFilePath);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
    }
}
