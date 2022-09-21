using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
    }
}
