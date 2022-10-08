using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ComTransfer
{
    /// <summary>
    /// 计划任务修改窗口
    /// </summary>
    public partial class TaskWindow : Window
    {
        /// <summary>
        /// 计划任务对象列表
        /// </summary>
        public ObservableCollection<TaskNode> TaskList { get; set; }
        /// <summary>
        /// 构建函数
        /// </summary>
        /// <param name="window">父窗口对象</param>
        public TaskWindow(Window window)
        {
            Owner = window;

            DataContext = this;

            TaskList = new ObservableCollection<TaskNode>(TaskManager.Instance.TaskList.Select(item => item.Clone()));

            InitializeComponent();
        }

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);

            TaskManager.SaveData(TaskList.ToList());
            TaskManager.SaveFailFileData(TaskManager.Instance.FailFileList);
        }

        /// <summary>
        /// 添加按钮点击事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            TaskList.Add(new TaskNode());
        }

        /// <summary>
        /// 删除按钮点击事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            TaskList.Remove(ListBox_Task.SelectedItem as TaskNode);
        }
    }
}
