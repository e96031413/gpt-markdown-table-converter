using System;
using System.Windows;
using System.Windows.Input;
using TextTableConverter.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection; 

namespace TextTableConverter.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IServiceProvider _serviceProvider; 
        private readonly ITextAnalysisService _textAnalysisService;

        public SettingsViewModel(ISettingsService settingsService, IServiceProvider serviceProvider) 
        {
            _settingsService = settingsService;
            _serviceProvider = serviceProvider; 
            _textAnalysisService = serviceProvider.GetRequiredService<ITextAnalysisService>();
            LoadSettings();
        }

        [ObservableProperty]
        private string _apiKey = string.Empty;

        partial void OnApiKeyChanged(string value)
        {
            System.Diagnostics.Debug.WriteLine($"ApiKey changed: {value}");
            Console.WriteLine($"ApiKey changed: {value}");
        }

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isSuccess;

        [ObservableProperty]
        private bool _isApiKeyValid;

        [RelayCommand]
        private async Task SaveSettings()
        {
            try 
            {
                // 保存 API Key
                if (string.IsNullOrWhiteSpace(ApiKey))
                {
                    StatusMessage = "API Key 不能為空";
                    return;
                }

                try
                {
                    _settingsService.SaveApiKey(ApiKey);
                    StatusMessage = "API Key 儲存成功";
                    Console.WriteLine($"API Key 儲存成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"儲存 API Key 時發生錯誤：{ex.Message}");
                    StatusMessage = $"儲存失敗：{ex.Message}";
                }

                // 测试 API Key 连接
                var connectionTestTask = _textAnalysisService.TestConnectionAsync();
                await connectionTestTask; // 异步等待连接测试完成

                if (connectionTestTask.Result)
                {
                    Console.WriteLine("API Key 連接測試成功");
                    StatusMessage = "API Key 保存成功";
                    IsApiKeyValid = true;
                    
                    CloseWindow();
                }
                else
                {
                    Console.WriteLine("API Key 連接測試失敗");
                    StatusMessage = "API Key 無效，請檢查後再試";
                    IsApiKeyValid = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存 API Key 時發生錯誤: {ex.Message}");
                StatusMessage = $"保存失敗: {ex.Message}";
                IsApiKeyValid = false;
            }
        }

        [RelayCommand]
        private void CancelSettings()
        {
            try 
            {
                // 恢复原始 API Key
                ApiKey = _settingsService.GetApiKey();
                
                // 关闭窗口
                CloseWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取消設置時發生錯誤: {ex.Message}");
                StatusMessage = $"取消失敗: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CloseWindow()
        {
            var window = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }

        private void LoadSettings()
        {
            try
            {
                ApiKey = _settingsService.GetApiKey();
            }
            catch (Exception ex)
            {
                StatusMessage = $"加載設置失敗: {ex.Message}";
                IsSuccess = false;
            }
        }
    }
}
