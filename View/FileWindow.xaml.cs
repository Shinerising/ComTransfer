using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComTransfer
{
    /// <summary>
    /// FileWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FileWindow : Window
    {
        public static List<FileNode> FileRoot { get; set; } = new List<FileNode>(){
            new FileNode(){ FileName = "远程计算机" }
        };
        public string Path { get; set; }
        public bool IsScrollDeferEnabled => Environment.OSVersion.Version.Major < 6;
        public static bool IsWaiting;

        public FileWindow(Window window)
        {
            Owner = window;

            DataContext = this;

            if (IsWaiting)
            {
                FileRoot = new List<FileNode>(){
                    new FileNode(){ FileName = "远程计算机" }
                };
                IsWaiting = false;
            }

            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }
        private void FileTree_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            FileNode node = item.DataContext as FileNode;
            if (sender != e.OriginalSource)
            {
                return;
            }
            if (node == null)
            {
                return;
            }
            if (node.IsFile)
            {
                return;
            }
            if (node.IsLoaded)
            {
                node.IsLoaded = false;
            }
        }
        private void FileTree_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            FileNode node = item.DataContext as FileNode;
            if (node == null)
            {
                e.Handled = true;
                return;
            }
            if (node.IsFile)
            {
                item.IsExpanded = false;
                TextBox_Path.Text = Path;
                DialogResult = true;
                e.Handled = true;
                return;
            }
            if (node.IsLoaded)
            {
                return;
            }
            if (IsWaiting || Border_Loading.Visibility == Visibility.Visible)
            {
                return;
            }
            IsWaiting = true;
            Border_Loading.Visibility = Visibility.Visible;
            MainWindow window = Owner as MainWindow;
            string root = node.FullName;
            window.LastCommand = null;
            window.SubmitCommand("requestfile", root);
            List<FileNode> list = new List<FileNode>();

            Task.Factory.StartNew(() =>
            {
                int i = 0;
                while (i < 300)
                {
                    if (window.LastCommand != null)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                }

                if (window.LastCommand != null)
                {
                    if (window.LastCommand.StartsWith("responsefile"))
                    {
                        string result = window.LastCommand.Substring(12).Trim();
                        int offset = result.IndexOf('|');
                        string filetree = offset != -1 ? result.Substring(offset + 1) : result;
                        list = filetree.Split('|').Where(option => option.Length != 0).Select(option => new FileNode(option, root)).ToList();
                    }
                }

                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    node.SetFileList(list);
                    Border_Loading.Visibility = Visibility.Collapsed;
                    IsWaiting = false;
                }));
            });
        }

        private void FileTree_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            Path = ((FileNode)item.DataContext).FullName;
            TextBox_Path.Text = Path;
            e.Handled = true;
        }

        private void Button_Submit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

    public class FileNode : CustomINotifyPropertyChanged
    {
        public string FileName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime ModifiedTime { get; set; }
        public long Length { get; set; }
        public string ToolTip
        {
            get
            {
                if (IsRoot)
                {
                    return "远程计算机根目录";
                }
                if (IsDisk)
                {
                    return string.Format("名称：{1}{0}类型：静态磁盘{0}磁盘空间：{2}", Environment.NewLine, FileName, GetSizeString(Length));
                }
                if (IsDirectory)
                {
                    return string.Format("名称：{1}{0}类型：文件夹{0}修改时间：{2}", Environment.NewLine, FileName, ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                return string.Format("名称：{1}{0}类型：文件{0}文件大小：{2}{0}修改时间：{3}", Environment.NewLine, FileName, GetSizeString(Length), ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
        public static string GetSizeString(long size)
        {
            if (size == 0)
            {
                return "0 Byte";
            }
            else if (size < 1024)
            {
                return string.Format("{0} Bytes", size);
            }
            else if (size < 1024 * 1024)
            {
                return string.Format("{0:f1} KB", (double)size / 1024);
            }
            else if (size < 1024 * 1024 * 1024)
            {
                return string.Format("{0:f1} MB", (double)size / (1024 * 1024));
            }
            else
            {
                return string.Format("{0:f1} GB", (double)size / (1024 * 1024 * 1024));
            }
        }
        public bool IsDirectory { get; set; }
        public bool IsDisk { get; set; }
        public bool IsRoot { get; set; }
        public bool IsFile { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsLoaded { get; set; }
        public List<FileNode> FileList { get; set; }
        public FileNode()
        {
            IsRoot = true;
            FileList = new List<FileNode>();
        }
        public FileNode(string option, string root)
        {
            if (option == null)
            {
                return;
            }
            try
            {
                if (option.StartsWith("D>"))
                {
                    int offset = 2;
                    {
                        int head = option.IndexOf('[', offset);
                        int tail = option.IndexOf(']', offset);
                        if (head != -1 && tail != -1 && tail > head)
                        {
                            Length = long.Parse(option.Substring(head + 1, tail - head - 1));
                            offset = tail + 1;
                        }
                    }
                    IsDisk = true;
                    FileName = option.Substring(offset);
                    FullName = FileName;
                    FileList = new List<FileNode>();
                }
                else if (option.StartsWith("F>"))
                {
                    IsDirectory = true;
                    int offset = 2;
                    {
                        int head = option.IndexOf('[', offset);
                        int tail = option.IndexOf(']', offset);
                        if (head != -1 && tail != -1 && tail > head)
                        {
                            ModifiedTime = DateTime.Parse(option.Substring(head + 1, tail - head - 1));
                            offset = tail + 1;
                        }

                    }
                    FileName = option.Substring(offset);
                    FullName = root.EndsWith("\\") ? root + FileName : root + "\\" + FileName;
                    FileList = new List<FileNode>();
                }
                else
                {
                    int offset = 0;
                    {
                        int head = option.IndexOf('[', offset);
                        int tail = option.IndexOf(']', offset);
                        if (head != -1 && tail != -1 && tail > head)
                        {
                            Length = long.Parse(option.Substring(head + 1, tail - head - 1));
                            offset = tail + 1;
                        }
                    }
                    {
                        int head = option.IndexOf('[', offset);
                        int tail = option.IndexOf(']', offset);
                        if (head != -1 && tail != -1 && tail > head)
                        {
                            ModifiedTime = DateTime.Parse(option.Substring(head + 1, tail - head - 1)).ToLocalTime();
                            offset = tail + 1;
                        }
                    }
                    IsFile = true;
                    FileName = option.Substring(offset);
                    FullName = root.EndsWith("\\") ? root + FileName : root + "\\" + FileName;
                }
            }
            catch
            {

            }
        }
        public void SetFileList(List<FileNode> list)
        {
            IsLoaded = true;
            FileList = list;
            Notify(new { FileList, IsLoaded });
        }
    }
}
