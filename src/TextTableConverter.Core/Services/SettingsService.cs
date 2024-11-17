using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using TextTableConverter.Core.Services.Interfaces;

namespace TextTableConverter.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly IConfiguration _configuration;

        public string OpenAIApiKey { get; set; } = string.Empty;
        public string GoogleDriveCredentials { get; set; } = string.Empty;
        public bool IsDarkMode { get; set; } = false;
        public bool ShowToolbar { get; set; } = true;

        public SettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TextTableConverter");
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
            LoadSettings();
        }

        public string GetApiKey()
        {
            // 首先檢查內存中的 API Key
            if (!string.IsNullOrEmpty(OpenAIApiKey))
            {
                Console.WriteLine($"Returning API Key from memory: {OpenAIApiKey}");
                return OpenAIApiKey;
            }

            // 嘗試從設置文件加載
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    Console.WriteLine($"Attempting to read settings from: {_settingsFilePath}");
                    var json = File.ReadAllText(_settingsFilePath);
                    Console.WriteLine($"Read JSON: {json}");

                    var settings = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                    
                    if (settings.TryGetProperty("ApiKey", out var apiKeyElement) && 
                        apiKeyElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        var apiKey = apiKeyElement.GetString() ?? string.Empty;
                        Console.WriteLine($"Found API Key in settings: {apiKey}");
                        OpenAIApiKey = apiKey;
                        return apiKey;
                    }
                    else
                    {
                        Console.WriteLine("No API Key found in settings file");
                    }
                }
                else
                {
                    Console.WriteLine($"Settings file does not exist: {_settingsFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading settings file: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            // 嘗試從環境變量加載
            var envApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrEmpty(envApiKey))
            {
                Console.WriteLine($"Found API Key in environment variable");
                OpenAIApiKey = envApiKey;
                return OpenAIApiKey;
            }

            // 嘗試從配置加載
            var configApiKey = _configuration["OpenAI:ApiKey"];
            if (!string.IsNullOrEmpty(configApiKey))
            {
                Console.WriteLine($"Found API Key in configuration");
                OpenAIApiKey = configApiKey;
                return OpenAIApiKey;
            }

            Console.WriteLine("No API Key found in any source");
            return string.Empty;
        }

        public void SaveApiKey(string apiKey)
        {
            Console.WriteLine($"Attempting to save API Key. Length: {apiKey?.Length ?? 0}");

            try 
            {
                // 確保目錄存在
                var settingsDir = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrWhiteSpace(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }

                // 創建一個包含所有當前設置的 JSON 對象
                var settings = new Dictionary<string, object?>
                {
                    { "ApiKey", apiKey ?? string.Empty },
                    { "IsDarkMode", IsDarkMode },
                    { "ShowToolbar", ShowToolbar },
                    { "GoogleDriveCredentials", GoogleDriveCredentials ?? string.Empty }
                };

                // 序列化並保存
                var jsonString = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                Console.WriteLine($"Saving JSON: {jsonString}");
                Console.WriteLine($"Saving to path: {_settingsFilePath}");

                File.WriteAllText(_settingsFilePath, jsonString);

                // 更新內存中的 API Key
                OpenAIApiKey = apiKey ?? string.Empty;

                Console.WriteLine("Settings saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; // 重新拋出異常，讓調用者知道保存失敗
            }
        }

        public string GetDefaultModel()
        {
            return _configuration["OpenAI:DefaultModel"] ?? "gpt-3.5-turbo-16k-0613";
        }

        public void SaveDefaultModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentException("Model name cannot be empty.", nameof(model));
            }

            // Save the model name to the settings file or configuration
            // Implementation can be added here if needed
        }

        public void SaveGoogleDriveCredentials(string credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials))
            {
                throw new ArgumentException("Credentials cannot be empty", nameof(credentials));
            }

            try
            {
                GoogleDriveCredentials = credentials;
                var settings = new { ApiKey = OpenAIApiKey, IsDarkMode = IsDarkMode, ShowToolbar = ShowToolbar, GoogleDriveCredentials = credentials };
                var json = System.Text.Json.JsonSerializer.Serialize(settings);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save Google Drive credentials: {ex.Message}", ex);
            }
        }

        public void ToggleDarkMode()
        {
            IsDarkMode = !IsDarkMode;
            var settings = new { ApiKey = OpenAIApiKey, IsDarkMode = IsDarkMode, ShowToolbar = ShowToolbar, GoogleDriveCredentials = GoogleDriveCredentials };
            var json = System.Text.Json.JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsFilePath, json);
        }

        public void ToggleToolbar()
        {
            ShowToolbar = !ShowToolbar;
            var settings = new { ApiKey = OpenAIApiKey, IsDarkMode = IsDarkMode, ShowToolbar = ShowToolbar, GoogleDriveCredentials = GoogleDriveCredentials };
            var json = System.Text.Json.JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsFilePath, json);
        }

        public void ClearApiKey()
        {
            // 清除内存中的 API Key
            OpenAIApiKey = string.Empty;

            // 尝试删除设置文件中的 API Key
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = System.Text.Json.JsonDocument.Parse(json);
                    var rootElement = settings.RootElement;

                    var builder = new System.Text.Json.Nodes.JsonObject();

                    // 复制除 ApiKey 外的所有属性
                    foreach (var property in rootElement.EnumerateObject())
                    {
                        if (property.Name != "ApiKey")
                        {
                            builder[property.Name] = System.Text.Json.Nodes.JsonNode.Parse(property.Value.GetRawText());
                        }
                    }

                    // 保存更新后的设置
                    var updatedJson = builder.ToJsonString();
                    File.WriteAllText(_settingsFilePath, updatedJson);

                    Console.WriteLine("API Key cleared from settings file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing API Key from settings: {ex.Message}");
            }

            // 清除环境变量
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);

            Console.WriteLine("API Key has been completely cleared");
        }

        private void LoadSettings()
        {
            try
            {
                Console.WriteLine($"Attempting to load settings from: {_settingsFilePath}");

                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    Console.WriteLine($"Read JSON: {json}");

                    var settings = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                    
                    if (settings.ValueKind != System.Text.Json.JsonValueKind.Undefined && 
                        settings.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        // 加載 API Key
                        if (settings.TryGetProperty("ApiKey", out var apiKeyElement) && 
                            apiKeyElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            var apiKey = apiKeyElement.GetString();
                            Console.WriteLine($"Loaded API Key from settings. Length: {apiKey?.Length ?? 0}");
                            
                            // 只有在 API Key 不為空時才設置
                            if (!string.IsNullOrWhiteSpace(apiKey))
                            {
                                OpenAIApiKey = apiKey;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No API Key found in settings");
                            OpenAIApiKey = string.Empty;
                        }

                        // 加載深色模式設置
                        if (settings.TryGetProperty("IsDarkMode", out var isDarkModeElement) && 
                            (isDarkModeElement.ValueKind == System.Text.Json.JsonValueKind.True || 
                             isDarkModeElement.ValueKind == System.Text.Json.JsonValueKind.False))
                        {
                            IsDarkMode = isDarkModeElement.GetBoolean();
                            Console.WriteLine($"Loaded Dark Mode setting: {IsDarkMode}");
                        }
                        else
                        {
                            Console.WriteLine("No Dark Mode setting found, using default");
                            IsDarkMode = false;
                        }

                        // 加載工具欄設置
                        if (settings.TryGetProperty("ShowToolbar", out var showToolbarElement) && 
                            (showToolbarElement.ValueKind == System.Text.Json.JsonValueKind.True || 
                             showToolbarElement.ValueKind == System.Text.Json.JsonValueKind.False))
                        {
                            ShowToolbar = showToolbarElement.GetBoolean();
                            Console.WriteLine($"Loaded Toolbar setting: {ShowToolbar}");
                        }
                        else
                        {
                            Console.WriteLine("No Toolbar setting found, using default");
                            ShowToolbar = true;
                        }

                        // 加載 Google Drive 憑據
                        if (settings.TryGetProperty("GoogleDriveCredentials", out var googleDriveCredentialsElement) && 
                            googleDriveCredentialsElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            GoogleDriveCredentials = googleDriveCredentialsElement.GetString() ?? string.Empty;
                            Console.WriteLine($"Loaded Google Drive Credentials. Length: {GoogleDriveCredentials.Length}");
                        }
                        else
                        {
                            Console.WriteLine("No Google Drive Credentials found");
                            GoogleDriveCredentials = string.Empty;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Settings file does not exist: {_settingsFilePath}");
                    // 設置默認值
                    OpenAIApiKey = string.Empty;
                    IsDarkMode = false;
                    ShowToolbar = true;
                    GoogleDriveCredentials = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // 發生錯誤時設置默認值
                OpenAIApiKey = string.Empty;
                IsDarkMode = false;
                ShowToolbar = true;
                GoogleDriveCredentials = string.Empty;
            }
        }
    }
}
