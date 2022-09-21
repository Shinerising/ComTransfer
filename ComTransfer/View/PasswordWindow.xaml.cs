using System.Windows;
using System.Windows.Controls;

namespace ComTransfer
{
    /// <summary>
    /// 用于密码确认的密码输入窗口
    /// </summary>
    public partial class PasswordWindow : Window
    {
        /// <summary>
        /// 窗口标题
        /// </summary>
        public string PasswordCaption { get; set; }
        /// <summary>
        /// 提示文字
        /// </summary>
        public string PasswordMessage { get; set; }
        /// <summary>
        /// 目标密码
        /// </summary>
        private readonly string PasswordTarget;
        /// <summary>
        /// 输入密码
        /// </summary>
        private string PasswordInput;
        /// <summary>
        /// 是否允许提示
        /// </summary>
        public bool IsHintAvailable { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">父窗口</param>
        /// <param name="message">提示信息</param>
        /// <param name="password">密码文本</param>
        /// <param name="caption">窗口标题</param>
        /// <param name="isHint">是否允许提示</param>
        public PasswordWindow(Window owner, string message, string password, string caption, bool isHint)
        {
            Loaded += PasswordWindow_Loaded;

            Owner = owner;
            PasswordCaption = caption;
            PasswordMessage = message;
            PasswordTarget = password;
            PasswordInput = string.Empty;
            IsHintAvailable = isHint;

            DataContext = this;

            InitializeComponent();

            PasswordBox_Normal.Focus();
        }

        private void PasswordWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordTarget))
            {
                Close();
            }
        }

        /// <summary>
        /// 显示密码窗口
        /// </summary>
        /// <param name="owner">父窗口</param>
        /// <param name="message">提示信息</param>
        /// <param name="password">密码文本</param>
        /// <param name="caption">窗口标题</param>
        /// <param name="isHint">是否允许提示</param>
        /// <returns>是否确认</returns>
        public static bool? Show(Window owner, string message, string password, string caption = null, bool isHint = false)
        {
            return new PasswordWindow(owner, message, password, caption, isHint).ShowDialog();
        }

        /// <summary>
        /// 确认按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Button_Submit_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordInput == PasswordTarget)
            {
                DialogResult = true;
            }
            else
            {
                Toast.IsEnabled = false;
                Toast.IsEnabled = true;
            }
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordInput = ((PasswordBox)sender).Password;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordInput = ((TextBox)sender).Text;
        }

        private void Button_View_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox_View.IsVisible)
            {
                PasswordBox_View.Visibility = Visibility.Collapsed;
                PasswordBox_Normal.Password = PasswordInput;
                PasswordBox_Normal.Focus();
            }
            else
            {
                PasswordBox_View.Visibility = Visibility.Visible;
                PasswordBox_View.Text = PasswordInput;
                PasswordBox_View.Focus();
            }
        }
    }
}
