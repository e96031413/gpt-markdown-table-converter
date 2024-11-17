namespace TextTableConverter.Core.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> ReadFileAsync(string filePath);
        Task SaveFileAsync(string filePath, string content);
        bool ValidateFilePath(string filePath);
        Task ExportTableAsync(string filePath, string[][] tableData);
    }
}
