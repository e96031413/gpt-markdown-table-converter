using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using TextTableConverter.Core.Services.Interfaces;
using TextTableConverter.Core.Services;
using TextTableConverter.UI.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Diagnostics;

namespace TextTableConverter.UI
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"UI線程異常：{e.Exception.Message}\n{e.Exception.StackTrace}");
            MessageBox.Show($"發生錯誤：{e.Exception.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine($"非UI線程異常：{ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"發生嚴重錯誤：{ex.Message}", "嚴重錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                _configuration = builder.Build();

                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                MainWindow = mainWindow;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"應用程序啟動錯誤：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // 註冊配置
                services.AddSingleton(_configuration!);

                // 註冊核心服務
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<ITextAnalysisService, TextAnalysisService>();

                // 註冊 ViewModel
                services.AddSingleton<MainViewModel>();

                // 註冊 MainWindow
                services.AddSingleton<MainWindow>();
            }
            catch (Exception ex)
            {
                throw new Exception($"服務配置錯誤：{ex.Message}", ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }
    }
}
