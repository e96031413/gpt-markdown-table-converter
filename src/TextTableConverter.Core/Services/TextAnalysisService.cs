using System;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Chat;
using TextTableConverter.Core.Services.Interfaces;
using TextTableConverter.Core.Models;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace TextTableConverter.Core.Services
{
    public class TextAnalysisService : ITextAnalysisService
    {
        private string? _apiKey;
        private string? _hashedApiKey;
        private OpenAIAPI? _api;
        private const int MinApiKeyLength = 32; // OpenAI API Key 通常很長

        public TextAnalysisService()
        {
            _api = null;
        }

        public void SetApiKey(string apiKey)
        {
            // 验证 API Key 的基本格式
            if (string.IsNullOrWhiteSpace(apiKey) || !IsValidApiKey(apiKey))
            {
                ClearApiKey();
                throw new ArgumentException("無效的 API Key");
            }

            // 清除之前的 API Key
            ClearApiKey();

            // 设置新的 API Key
            _apiKey = apiKey; 
            _hashedApiKey = HashApiKey(apiKey); // 存儲 API Key 的哈希值
            _api = new OpenAIAPI(apiKey);

            // 记录日志
            Console.WriteLine("New API Key has been set and initialized");
        }

        private bool IsValidApiKey(string apiKey)
        {
            // 基本的 API Key 驗證
            return !string.IsNullOrWhiteSpace(apiKey) && 
                   apiKey.Length >= MinApiKeyLength && 
                   apiKey.StartsWith("sk-");
        }

        private string HashApiKey(string apiKey)
        {
            // 使用安全的哈希方法保護 API Key
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public void ClearApiKey()
        {
            // 完全清除 API Key 相关信息
            _apiKey = null;
            _hashedApiKey = null;
            _api = null;

            // 记录日志
            Console.WriteLine("API Key has been completely cleared from memory");
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (_api == null)
            {
                return false;
            }

            try
            {
                var chatRequest = new ChatRequest
                {
                    Model = "gpt-4o-mini", // 使用 gpt-4o-mini 模型
                    Messages = new List<ChatMessage>
                    {
                        new ChatMessage(ChatMessageRole.User, "Connection test")
                    }
                };

                await _api.Chat.CreateChatCompletionAsync(chatRequest);
                return true;
            }
            catch (Exception)
            {
                ClearApiKey();
                return false;
            }
        }

        public async Task<string[][]> AnalyzeTextAsync(string inputText)
        {
            if (_api == null)
            {
                throw new InvalidOperationException("API Key is required for text analysis.");
            }

            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.AppendSystemMessage(@"You are a text analysis assistant. Your task is to:
                                            1. Analyze the input text
                                            2. Extract key information
                                            3. Organize the information into a 2D string array
                                            4. Ensure the array represents meaningful data structure
                                            5. Do not include any explanations or additional text");

                chat.AppendUserInput($"Analyze this text and return a 2D array:\n{inputText}");
                var response = await chat.GetResponseFromChatbotAsync();

                var rows = response.Trim().Split('\n');
                var result = new string[rows.Length][];
                for (int i = 0; i < rows.Length; i++)
                {
                    result[i] = rows[i].Split('|', StringSplitOptions.RemoveEmptyEntries);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Text analysis failed: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateTextAsync(string[][] tableData)
        {
            if (_api == null)
            {
                throw new InvalidOperationException("API Key is required for text generation.");
            }

            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.AppendSystemMessage(@"You are a text generation assistant. Your task is to:
                                        1. Convert the input 2D array into a coherent text description.
                                        2. Preserve all information from the array.
                                        3. Create well-structured, readable text.
                                        4. Avoid any additional formatting or explanations.
                                        5. Respond in the language of the input."
                                        );

                var tableString = string.Join("\n", 
                    Array.ConvertAll(tableData, row => string.Join(" | ", row)));

                chat.AppendUserInput($"Generate text from this data:\n{tableString}");
                var response = await chat.GetResponseFromChatbotAsync();

                return response.Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Text generation failed: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> ConvertTextToTableAsync(string text)
        {
            if (_api == null)
            {
                throw new InvalidOperationException("API Key is required for text to table conversion.");
            }

            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.AppendSystemMessage(@"You are a text-to-table conversion assistant. Your task is to:
                                        1. Convert the input text into a structured DataTable
                                        2. Identify meaningful columns
                                        3. Ensure data is correctly parsed
                                        4. Return a valid, meaningful table structure");

                chat.AppendUserInput($"Convert this text to a table:\n{text}");
                var response = await chat.GetResponseFromChatbotAsync();

                var dataTable = new DataTable();
                var rows = response.Trim().Split('\n');
                
                var headers = rows[0].Split('|', StringSplitOptions.RemoveEmptyEntries);
                foreach (var header in headers)
                {
                    dataTable.Columns.Add(header.Trim());
                }

                for (int i = 1; i < rows.Length; i++)
                {
                    var rowData = rows[i].Split('|', StringSplitOptions.RemoveEmptyEntries);
                    dataTable.Rows.Add(rowData);
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Text to table conversion failed: {ex.Message}", ex);
            }
        }

        public async Task<string> ConvertTableToTextAsync(DataTable table)
        {
            if (_api == null)
            {
                throw new InvalidOperationException("API Key is required for table to text conversion.");
            }

            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.AppendSystemMessage(@"You are a text generation assistant. Your task is to:
                                        1. Convert the input 2D array into a coherent text description.
                                        2. Preserve all information from the array.
                                        3. Create well-structured, readable text.
                                        4. Avoid any additional formatting or explanations.
                                        5. Respond in the language of the input."
                                        );

                var tableString = new System.Text.StringBuilder();
                
                tableString.AppendLine(string.Join(" | ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
                
                foreach (DataRow row in table.Rows)
                {
                    tableString.AppendLine(string.Join(" | ", row.ItemArray));
                }

                chat.AppendUserInput($"Generate text from this table:\n{tableString}");
                var response = await chat.GetResponseFromChatbotAsync();

                return response.Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Table to text conversion failed: {ex.Message}", ex);
            }
        }

        public async Task<string> AnalyzeTableStructureAsync(DataTable table)
        {
            if (_api == null)
            {
                throw new InvalidOperationException("API Key is required for table structure analysis.");
            }

            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.AppendSystemMessage(@"You are a text generation assistant. Your task is to:
                                        1. Convert the input 2D array into a coherent text description.
                                        2. Preserve all information from the array.
                                        3. Create well-structured, readable text.
                                        4. Avoid any additional formatting or explanations.
                                        5. Respond in the language of the input.");

                var tableString = new System.Text.StringBuilder();
                
                tableString.AppendLine("Columns:");
                foreach (DataColumn column in table.Columns)
                {
                    tableString.AppendLine($"{column.ColumnName}: {column.DataType.Name}");
                }
                
                tableString.AppendLine($"Total Rows: {table.Rows.Count}");

                chat.AppendUserInput($"Analyze this table structure:\n{tableString}");
                var response = await chat.GetResponseFromChatbotAsync();

                return response.Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Table structure analysis failed: {ex.Message}", ex);
            }
        }

        public async Task<Result<string>> TextToTableAsync(string text)
        {
            Console.WriteLine($"TextToTableAsync called. Input length: {text?.Length ?? 0}");
            Console.WriteLine($"Current API Key status: {(_apiKey != null ? "Present" : "Empty")}");

            if (_api == null)
            {
                return Result<string>.Failure("未設置 API Key");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return Result<string>.Failure("輸入文本內容不能為空");
            }

            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.AppendSystemMessage(@"你是一個文本轉換助手。你的任務是：
                                        1. 將輸入的文本轉換為結構化的表格
                                        2. 識別文本中的關鍵信息
                                        3. 創建一個清晰、有意義的表格
                                        4. 返回表格的字符串表示
                                        5. 不要包含任何額外的解釋或格式
                                        6. 使用輸入的語言進行回復");

                chat.AppendUserInput($"將以下文本轉換為表格：\n{text}");
                var response = await chat.GetResponseFromChatbotAsync();

                return Result<string>.Success(response.Trim());
            }
            catch (Exception ex)
            {
                ClearApiKey(); // 發生錯誤時清除 API Key
                return Result<string>.Failure($"轉換失敗：{ex.Message}");
            }
        }

        public async Task<Result<string>> TableToTextAsync(string markdownTable)
        {
            Console.WriteLine($"TableToTextAsync called. Input length: {markdownTable?.Length ?? 0}");
            Console.WriteLine($"Current API Key status: {(_apiKey != null ? "Present" : "Empty")}");

            if (_api == null)
            {
                return Result<string>.Failure("未設置 API Key");
            }

            if (string.IsNullOrWhiteSpace(markdownTable))
            {
                return Result<string>.Failure("輸入表格不能為空");
            }

            try
            {
                var chat = _api.Chat.CreateConversation();
                chat.AppendSystemMessage(@"你是一個表格轉換助手。你的任務是：
                                        1. 將輸入的表格轉換為自然語言文本
                                        2. 保留表格中的所有信息
                                        3. 創建清晰、易讀的文本
                                        4. 不要包含任何額外的解釋或格式");

                chat.AppendUserInput($"將以下表格轉換為文本：\n{markdownTable}");
                var response = await chat.GetResponseFromChatbotAsync();

                return Result<string>.Success(response.Trim());
            }
            catch (Exception ex)
            {
                ClearApiKey(); // 發生錯誤時清除 API Key
                return Result<string>.Failure($"轉換失敗：{ex.Message}");
            }
        }
    }
}
