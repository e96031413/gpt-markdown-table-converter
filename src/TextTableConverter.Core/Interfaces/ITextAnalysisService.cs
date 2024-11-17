using System.Threading.Tasks;
using System.Data;

namespace TextTableConverter.Core.Interfaces
{
    public interface ITextAnalysisService
    {
        /// <summary>
        /// 設置 OpenAI API 金鑰
        /// </summary>
        void SetApiKey(string apiKey);

        /// <summary>
        /// 測試 API 連接是否正常
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// 將輸入的文字轉換為結構化表格數據
        /// </summary>
        Task<string[][]> AnalyzeTextAsync(string inputText);

        /// <summary>
        /// 將表格數據轉換為自然語言文本
        /// </summary>
        Task<string> GenerateTextAsync(string[][] tableData);

        /// <summary>
        /// 將文字轉換為表格格式
        /// </summary>
        Task<DataTable> ConvertTextToTableAsync(string text);

        /// <summary>
        /// 將表格轉換為自然文字
        /// </summary>
        Task<string> ConvertTableToTextAsync(DataTable table);

        /// <summary>
        /// 分析表格結構並提供見解
        /// </summary>
        Task<string> AnalyzeTableStructureAsync(DataTable table);
    }
}
