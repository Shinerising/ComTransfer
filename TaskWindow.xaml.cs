using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ComTransfer
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TaskWindow : Window
    {
        public ObservableCollection<TaskNode> TaskList { get; set; }
        public TaskWindow(Window window)
        {
            Owner = window;

            DataContext = this;

            TaskList = TaskManager.Instance.TaskList;

            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void ListBox_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}
