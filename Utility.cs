using System.ComponentModel;
using System.Reflection;

namespace ComTransfer
{
    /// <summary>
    /// 用于单向数据绑定项目的数据变更过程实现
    /// </summary>
    public class CustomINotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性变化时的事件处理
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知UI更新数据的方法
        /// </summary>
        /// <typeparam name="T">泛型参数</typeparam>
        /// <param name="obj">以待更新项目为属性的匿名类实例</param>
        protected void Notify<T>(T obj)
        {
            if (obj == null)
            {
                return;
            }
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.Name));
            }
        }

        /// <summary>
        /// 通知UI更新数据的方法
        /// </summary>
        /// <param name="name">待更新的属性名称</param>
        protected void Notify(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
