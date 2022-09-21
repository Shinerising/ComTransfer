using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ComTransfer
{
    /// <summary>
    /// 能够在数据项更新时自动滚动至列表底部的ListBox行为实现
    /// </summary>
    public class ListBoxBehavior
    {
        /// <summary>
        /// 自定义属性，用于设置是否本行为启用
        /// </summary>
        public bool ScrollOnNewItem { get; set; }

        /// <summary>
        /// 获得自定义属性的值
        /// </summary>
        /// <param name="obj">控件对象</param>
        /// <returns>属性值</returns>
        public static bool GetScrollOnNewItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollOnNewItemProperty);
        }

        /// <summary>
        /// 设置自定义属性的值
        /// </summary>
        /// <param name="obj">控件对象</param>
        /// <param name="value">属性值</param>
        public static void SetScrollOnNewItem(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollOnNewItemProperty, value);
        }

        /// <summary>
        /// 自定义属性：<c>ScrollOnNewItem</c>，用于设置是否在增加新项目时滚动到底部
        /// </summary>
        public static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollOnNewItem",
                typeof(bool),
                typeof(ListBoxBehavior),
                new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

        /// <summary>
        /// 自定义属性设置响应
        /// </summary>
        /// <param name="sender">控件对象</param>
        /// <param name="e">事件参数</param>
        public static void OnScrollOnNewItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                ((ListBox)sender).Loaded += ListBox_Loaded;
            }
        }

        /// <summary>
        /// 列表加载事件响应
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            new ListBoxScroll(sender as ListBox);
        }

        /// <summary>
        /// 用于向列表框绑定事件的私有类
        /// </summary>
        private class ListBoxScroll
        {
            private readonly ListBox listBox;
            private ScrollViewer scrollViewer;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="listBox">列表框对象</param>
            public ListBoxScroll(ListBox listBox)
            {
                if (listBox != null && listBox.ItemsSource != null)
                {
                    this.listBox = listBox;
                    if (listBox.ItemsSource is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)listBox.ItemsSource).CollectionChanged += CollectionChanged;
                    }
                }
            }

            /// <summary>
            /// 列表框的数据源项目增加时将控件滚动至底部
            /// </summary>
            /// <param name="sender">事件对象</param>
            /// <param name="e">事件参数</param>
            private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add && !listBox.IsMouseOver)
                {
                    if (scrollViewer == null)
                    {
                        scrollViewer = GetDescendantByType(listBox, typeof(ScrollViewer)) as ScrollViewer;
                    }
                    scrollViewer?.ScrollToBottom();
                }
            }

            private static Visual GetDescendantByType(Visual element, Type type)
            {
                if (element == null)
                {
                    return null;
                }
                if (element.GetType() == type)
                {
                    return element;
                }
                Visual foundElement = null;
                if (element is FrameworkElement)
                {
                    (element as FrameworkElement).ApplyTemplate();
                }
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                {
                    Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                    foundElement = GetDescendantByType(visual, type);
                    if (foundElement != null)
                    {
                        break;
                    }
                }
                return foundElement;
            }
        }
    }
}