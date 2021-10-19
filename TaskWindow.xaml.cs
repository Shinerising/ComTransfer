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

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            TaskList.Add(new TaskNode());
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            TaskList.Remove(ListBox_Task.SelectedItem as TaskNode);
        }
    }
}
