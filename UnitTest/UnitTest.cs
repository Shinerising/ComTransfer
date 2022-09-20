using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void PasswordWindowTest()
        {
            PasswordWindow window = null;
            bool? result = null;

            var t = new Thread(() =>
            {
                window = new PasswordWindow(null, "", "123456", "", false);
                Assert.IsNotNull(window);
                result = window.ShowDialog();
                System.Windows.Threading.Dispatcher.Run();
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            while (window == null)
            {
                Thread.Sleep(10);
            }

            window.Dispatcher.Invoke(() =>
            {
                Button button = window.FindName("Button_Submit") as Button;
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.IsTrue(window.IsVisible);

                PasswordBox passwordBox = window.FindName("PasswordBox_Normal") as PasswordBox;
                passwordBox.Password = "123456";
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.IsFalse(window.IsVisible);
            });

            Assert.IsTrue(result);
        }
    }
}
