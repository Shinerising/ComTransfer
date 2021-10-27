using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
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
        /// <summary>
        /// 启动运行注册表位置
        /// </summary>
        private static readonly RegistryKey regPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
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
        public bool IsAutoRun { get; set; }
        public bool IsAutoStart
        {
            get
            {
                return regPath.GetValue(Process.GetCurrentProcess().ProcessName) != null;
            }
            set
            {
                if (value)
                {
                    regPath.SetValue(Process.GetCurrentProcess().ProcessName, Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    regPath.DeleteValue(Process.GetCurrentProcess().ProcessName, false);
                }
            }
        }
        public ConfigWindow(Window window)
        {
            Owner = window;

            DataContext = this;

            PortID = int.Parse(ConfigurationManager.AppSettings["com"]);
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]);
            DataBits = int.Parse(ConfigurationManager.AppSettings["databits"]);
            StopBits = int.Parse(ConfigurationManager.AppSettings["stopbits"]);
            Parity = ConfigurationManager.AppSettings["parity"].ToUpper();
            IsHW = ConfigurationManager.AppSettings["ishw"].ToUpper() == "TRUE";
            IsSW = ConfigurationManager.AppSettings["issw"].ToUpper() == "TRUE";
            IsDTR = ConfigurationManager.AppSettings["isdtr"].ToUpper() == "TRUE";
            IsRTS = ConfigurationManager.AppSettings["isrts"].ToUpper() == "TRUE";

            FolderConfig = ConfigurationManager.AppSettings["directory"];

            IsAutoRun = ConfigurationManager.AppSettings["autorun"].ToUpper() == "TRUE";

            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);

            SaveAllConfig();
        }
        public void SaveAllConfig()
        {
            SaveConfig(new Dictionary<string, object>()
            {
                { "com", PortID },
                { "baudrate", BaudRate },
                { "databits", DataBits },
                { "stopbits", StopBits },
                { "parity", Parity },
                { "ishw", IsHW },
                { "issw", IsSW },
                { "isdtr", IsDTR },
                { "isrts", IsRTS },
                { "directory", FolderConfig },
                { "autorun", IsAutoRun },
            });
        }

        private void SaveConfig(Dictionary<string, object> dict)
        {
            try
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                foreach (var pair in dict)
                {
                    object param = pair.Value;
                    string key = pair.Key;
                    if (param != null)
                    {
                        if (configuration.AppSettings.Settings.AllKeys.Contains(key))
                        {
                            configuration.AppSettings.Settings[key].Value = param.ToString();
                        }
                        else
                        {

                            configuration.AppSettings.Settings.Add(key, param.ToString());
                        }
                    }
                    else
                    {
                        configuration.AppSettings.Settings.Remove(key);
                    }
                }
                configuration.Save(ConfigurationSaveMode.Minimal, true);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch
            {

            }
        }
    }
}
