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
            new FileNode(){ FileName = "远端计算机" }
        };
        public string Path { get; set; }

        public FileWindow(Window window)
        {
            Owner = window;

            DataContext = this;

            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }
        private void FileTree_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            FileNode node = item.DataContext as FileNode;
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
                TextBox_Path.Text = Path;
                DialogResult = true;
                e.Handled = true;
                return;
            }
            if (node.IsLoaded)
            {
                return;
            }
            if (Border_Loading.Visibility == Visibility.Visible)
            {
                return;
            }
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
                        list = result.Split('|').Where(option => option.Length != 0).Select(option => new FileNode(option, root)).ToList();
                        
                    }
                }

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    node.SetFileList(list);
                    Border_Loading.Visibility = Visibility.Collapsed;
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
            if (option.StartsWith("D>"))
            {
                IsDisk = true;
                FileName = option.Substring(2);
                FullName = FileName;
                FileList = new List<FileNode>();
                return;
            }
            if (option.StartsWith("F>"))
            {
                IsDirectory = true;
                FileName = option.Substring(2);
                FullName = root.EndsWith("\\") ? root + FileName : root + "\\" + FileName;
                FileList = new List<FileNode>();
                return;
            }
            IsFile = true;
            FileName = option;
            FullName = root.EndsWith("\\") ? root + FileName : root + "\\" + FileName;
            return;
        }
        public void SetFileList(List<FileNode> list)
        {
            IsLoaded = true;
            FileList = list;
            Notify(new { FileList, IsLoaded });
        }
    }
}
