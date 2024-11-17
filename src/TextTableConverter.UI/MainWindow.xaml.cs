using System.Windows;
using TextTableConverter.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace TextTableConverter.UI
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            try
            {
                Console.WriteLine("MainWindow 構造函數開始...");
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

                // 添加異常處理
                Dispatcher.UnhandledException += Dispatcher_UnhandledException;

                Console.WriteLine("正在初始化組件...");
                InitializeComponent();
                Console.WriteLine("組件初始化完成");

                DataContext = _viewModel;
                Loaded += MainWindow_Loaded;
                Console.WriteLine("MainWindow 構造函數完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainWindow 構造函數錯誤：{ex.Message}\n{ex.StackTrace}");
                Debug.WriteLine($"MainWindow 構造函數錯誤：{ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"窗口初始化錯誤：{ex.Message}\n\n{ex.StackTrace}", 
                              "錯誤", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                throw;
            }
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"MainWindow Dispatcher 異常：{e.Exception.Message}\n{e.Exception.StackTrace}");
            MessageBox.Show($"窗口錯誤：{e.Exception.Message}\n\n{e.Exception.StackTrace}", 
                          "錯誤", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            e.Handled = true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("MainWindow_Loaded 事件觸發");
                _viewModel.StatusMessage = "應用程序已準備就緒";
                Console.WriteLine("MainWindow_Loaded 事件完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainWindow_Loaded 錯誤：{ex.Message}\n{ex.StackTrace}");
                Debug.WriteLine($"MainWindow_Loaded 錯誤：{ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"窗口加載錯誤：{ex.Message}\n\n{ex.StackTrace}", 
                              "錯誤", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                base.OnSourceInitialized(e);
                Console.WriteLine("MainWindow OnSourceInitialized 完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainWindow OnSourceInitialized 錯誤：{ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"窗口初始化錯誤：{ex.Message}\n\n{ex.StackTrace}", 
                              "錯誤", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }
    }
}