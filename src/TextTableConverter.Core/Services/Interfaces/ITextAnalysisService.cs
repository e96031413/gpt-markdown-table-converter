using System.Threading.Tasks;
using System.Data;
using TextTableConverter.Core.Models;

namespace TextTableConverter.Core.Services.Interfaces
{
    public interface ITextAnalysisService
    {
        /// <summary>
        /// 設置 OpenAI API 金鑰
        /// </summary>
        void SetApiKey(string apiKey);

        /// <summary>
        /// 清除當前的 API 金鑰
        /// </summary>
        void ClearApiKey();

        /// <summary>
        /// 測試 API 連接是否正常
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// 將文字轉換為表格格式
        /// </summary>
        Task<Result<string>> TextToTableAsync(string text);

        /// <summary>
        /// 將表格轉換為自然文字
        /// </summary>
        Task<Result<string>> TableToTextAsync(string markdownTable);
    }
}
