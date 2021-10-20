using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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

            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);

            SaveAllConfig();
        }
        public void SaveAllConfig()
        {
            SaveConfig(PortID, "com");
            SaveConfig(BaudRate, "baudrate");
            SaveConfig(DataBits, "databits");
            SaveConfig(StopBits, "stopbits");
            SaveConfig(Parity, "parity");
            SaveConfig(IsHW, "ishw");
            SaveConfig(IsSW, "issw");
            SaveConfig(IsDTR, "isdtr");
            SaveConfig(IsRTS, "isrts");
            SaveConfig(FolderConfig, "directory");
        }

        private void SaveConfig<T>(T param, string key)
        {
            try
            {
                if (param != null)
                {
                    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (configuration.AppSettings.Settings.AllKeys.Contains(key))
                    {
                        configuration.AppSettings.Settings[key].Value = param.ToString();
                    }
                    else
                    {

                        configuration.AppSettings.Settings.Add(key, param.ToString());
                    }
                    configuration.Save(ConfigurationSaveMode.Minimal, true);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                else
                {
                    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    configuration.AppSettings.Settings.Remove(key);
                    configuration.Save(ConfigurationSaveMode.Minimal, true);
                    ConfigurationManager.RefreshSection("appSettings");
                }
            }
            catch
            {

            }
        }
    }
}
