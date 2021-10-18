using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public int PortID { get; set; } = 1;
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public int StopBits { get; set; } = 1;
        public string Parity { get; set; } = "NONE";
        public bool IsHW { get; set; } = false;
        public bool IsSW { get; set; } = false;
        public bool IsDTR { get; set; } = true;
        public bool IsRTS { get; set; } = true;
        public string FolderConfig { get; set; } = "";
        public ConfigWindow(Window window)
        {
            Owner = window;

            DataContext = this;

            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
