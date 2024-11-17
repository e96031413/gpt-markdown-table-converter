using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TextTableConverter.Core.Services.Interfaces;
using TextTableConverter.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace TextTableConverter.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ITextAnalysisService _textAnalysisService;
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private string _inputText = string.Empty;

        [ObservableProperty]
        private string _tableMarkdown = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isProcessing;

        [ObservableProperty]
        private string _apiKey = string.Empty;

        public MainViewModel(ITextAnalysisService textAnalysisService, ISettingsService settingsService)
        {
            _textAnalysisService = textAnalysisService;
            _settingsService = settingsService;

            // 在构造函数中自动设置 API Key
            var apiKey = _settingsService.GetApiKey();
            Console.WriteLine($"初始化 MainViewModel。API Key 长度：{apiKey?.Length ?? 0}");

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                try 
                {
                    _textAnalysisService.SetApiKey(apiKey);
                    StatusMessage = "API Key 已自動載入";
                    Console.WriteLine("API Key 載入成功");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"API Key 載入失敗：{ex.Message}";
                    Console.WriteLine($"API Key 載入失敗：{ex.Message}");
                }
            }
            else
            {
                StatusMessage = "請輸入 API Key";
                Console.WriteLine("没有找到 API Key");
            }
        }

        private async Task ExecuteAsync(Func<Task> action, string processingMessage)
        {
            try
            {
                StatusMessage = processingMessage;
                IsProcessing = true;
                await action();
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task TextToTableAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                StatusMessage = "请输入文本";
                return;
            }

            await ExecuteAsync(async () =>
            {
                try
                {
                    var result = await _textAnalysisService.TextToTableAsync(InputText);
                    if (result.IsSuccess)
                    {
                        TableMarkdown = result.Value ?? string.Empty;
                        StatusMessage = "轉換完成";
                    }
                    else
                    {
                        StatusMessage = $"轉換失敗：{result.Error}";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"發生錯誤：{ex.Message}";
                }
            }, "正在轉換...");
        }

        [RelayCommand]
        private async Task TableToTextAsync()
        {
            if (string.IsNullOrWhiteSpace(TableMarkdown))
            {
                StatusMessage = "請先轉換表格";
                return;
            }

            await ExecuteAsync(async () =>
            {
                try
                {
                    var result = await _textAnalysisService.TableToTextAsync(TableMarkdown);
                    if (result.IsSuccess)
                    {
                        InputText = result.Value ?? string.Empty;
                        StatusMessage = "轉換完成";
                    }
                    else
                    {
                        StatusMessage = $"轉換失敗：{result.Error}";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"發生錯誤：{ex.Message}";
                }
            }, "正在轉換...");
        }

        [RelayCommand]
        private async Task OpenFileAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "文字檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*",
                Title = "選擇要轉換的檔案"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsProcessing = true;
                    StatusMessage = "正在開啟檔案...";

                    var fileContent = await File.ReadAllTextAsync(openFileDialog.FileName);
                    InputText = fileContent ?? string.Empty;  // 確保 InputText 不為 null

                    // 嘗試自動轉換
                    if (!string.IsNullOrWhiteSpace(InputText))
                    {
                        await TextToTableAsync();
                    }

                    StatusMessage = $"成功開啟檔案：{Path.GetFileName(openFileDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"開啟檔案失敗：{ex.Message}";
                    InputText = string.Empty;  // 確保 InputText 不為 null
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        [RelayCommand]
        private void SaveApiKey()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                StatusMessage = "API Key 不能為空";
                return;
            }

            try 
            {
                // 驗證 API Key 的基本格式
                if (!ApiKey.StartsWith("sk-") || ApiKey.Length < 32)
                {
                    StatusMessage = "無效的 API Key 格式";
                    return;
                }

                _textAnalysisService.SetApiKey(ApiKey);
                _settingsService.SaveApiKey(ApiKey);
                StatusMessage = "API Key 儲存成功";
                
                // 測試 API Key 的有效性
                TestApiKeyValidity();
            }
            catch (Exception ex)
            {
                StatusMessage = $"儲存 API Key 失敗：{ex.Message}";
            }
        }

        [RelayCommand]
        private void ClearApiKey()
        {
            ApiKey = string.Empty;
            _textAnalysisService.ClearApiKey();
            _settingsService.ClearApiKey();
            StatusMessage = "API Key 已清除";
            
            // 確保完全忘記之前的 Key
            Console.WriteLine("API Key 已從記憶體中清除");
        }

        private async void TestApiKeyValidity()
        {
            try 
            {
                // 如果 API Key 為空，直接返回
                if (string.IsNullOrWhiteSpace(ApiKey))
                {
                    StatusMessage = "API Key 不能為空";
                    return;
                }

                // 嘗試進行一個簡單的轉換來測試 API Key
                var testResult = await _textAnalysisService.TextToTableAsync("測試 API Key 有效性");
                
                if (testResult != null && testResult.IsSuccess)
                {
                    StatusMessage = "API Key 驗證成功";
                }
                else
                {
                    StatusMessage = testResult?.Error ?? "API Key 驗證失敗";
                    ClearApiKey();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"API Key 驗證錯誤：{ex.Message}";
                ClearApiKey();
            }
        }

        [RelayCommand]
        private void OpenMarkdownPreview()
        {
            try
            {
                var url = "https://markdownlivepreview.com/";
                var startInfo = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                StatusMessage = "已開啟 Markdown 預覽網站";
            }
            catch (Exception ex)
            {
                StatusMessage = $"開啟預覽網站失敗：{ex.Message}";
            }
        }
    }
}
