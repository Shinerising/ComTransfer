using ComTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ComTransfer.Tests
{
    [TestClass()]
    public class UnitTest
    {
        private static bool IsInitialized;
        public static void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }
            IsInitialized = true;

            Application.ResourceAssembly = Assembly.GetAssembly(typeof(App));
            var app = new App();
            app.InitializeComponent();
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
            }
        }
        public UnitTest()
        {
            Initialize();
        }
        [TestMethod()]
        public void SendFileTest()
        {
        }

        [TestMethod()]
        public void SetPortTest()
        {
        }

        [TestMethod()]
        public void MainWindowTest()
        {
            MainWindow window = new MainWindow();
            Assert.IsNotNull(window);
        }

        [TestMethod()]
        public void SubmitCommandTest()
        {
        }

        [TestMethod()]
        public void PasswordWindowTest01()
        {
            Assert.IsFalse(PasswordWindow.Show(null, "", ""));

            PasswordWindow window = new PasswordWindow(null, "", "123456", "", false);
            Assert.IsNotNull(window);
            window.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Button button = window.FindName("Button_Cancel") as Button;
                Assert.IsNotNull(button);
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            };
            Assert.IsFalse(window.ShowDialog());
        }
        [TestMethod()]
        public void PasswordWindowText02()
        {
            PasswordWindow window = new PasswordWindow(null, "", "123456", "", false);
            window.Loaded += (object sender, RoutedEventArgs e) =>
            {
                PasswordBox passwordBox = window.FindName("PasswordBox_Normal") as PasswordBox;
                TextBox textBox = window.FindName("PasswordBox_View") as TextBox;
                Button button = window.FindName("Button_Submit") as Button;
                Button toggleButton = window.FindName("Button_Toggle") as Button;
                Assert.IsNotNull(passwordBox);
                Assert.IsNotNull(textBox);
                Assert.IsNotNull(button);
                Assert.IsNotNull(toggleButton);

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.IsTrue(window.IsVisible);

                toggleButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.IsTrue(textBox.IsVisible);
                textBox.Text = "12345";
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.IsTrue(window.IsVisible);

                toggleButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.IsTrue(passwordBox.IsVisible);
                passwordBox.Password = "123456";
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.IsFalse(window.IsVisible);
            };
            Assert.IsTrue(window.ShowDialog());
        }

        [TestMethod()]
        public void ConfigWindowTest()
        {
            ConfigWindow window = new ConfigWindow(null);
            window.Show();
            var list01 = FindVisualChildren<TextBox>(window).ToList();
            Assert.AreEqual(list01[0].Text, "1");
            Assert.AreEqual(list01[1].Text, "8");
            Assert.AreEqual(list01[2].Text, "1");
            Assert.AreEqual(list01[3].Text, "115200");
            Assert.AreEqual(list01[4].Text, "NONE");
            Assert.AreEqual(list01[5].Text, @"*>C:\temp");

            var list02 = FindVisualChildren<CheckBox>(window).ToList();
            Assert.AreEqual(list02[0].IsChecked, false);
            Assert.AreEqual(list02[1].IsChecked, false);
            Assert.AreEqual(list02[2].IsChecked, true);
            Assert.AreEqual(list02[3].IsChecked, true);
            Assert.AreEqual(list02[4].IsChecked, false);
            Assert.AreEqual(list02[5].IsChecked, false);

            window.IsAutoStart = true;
            Assert.IsTrue(window.IsAutoStart);
            window.IsAutoStart = false;
            Assert.IsFalse(window.IsAutoStart);

            window.Close();
        }

        [TestMethod()]
        public void TaskWindowTest()
        {
            TaskWindow window = new TaskWindow(null);
            window.Show();
            ListBox listBox = window.FindName("ListBox_Task") as ListBox;
            Button button01 = window.FindName("Button_Add") as Button;
            Button button02 = window.FindName("Button_Delete") as Button;
            Assert.IsNotNull(listBox);
            Assert.IsNotNull(button01);
            Assert.IsNotNull(button02);

            Assert.AreEqual(listBox.Items.Count, 1);
            button01.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(listBox.Items.Count, 2);
            listBox.SelectedItem = listBox.Items[1];
            button02.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(listBox.Items.Count, 1);

            window.Close();
        }
    }
}
