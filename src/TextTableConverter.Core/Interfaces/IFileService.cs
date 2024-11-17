using System.Collections.ObjectModel;

namespace TextTableConverter.Core.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// 讀取檔案內容
        /// </summary>
        Task<string> ReadFileAsync(string filePath);

        /// <summary>
        /// 儲存檔案
        /// </summary>
        Task SaveFileAsync(string filePath, ObservableCollection<object> data);

        /// <summary>
        /// 匯出表格
        /// </summary>
        Task ExportTableAsync(object data, string filePath, string format);
    }
}
