using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using NPOI.XWPF.UserModel;
using NPOI.XSSF.UserModel;
using TextTableConverter.Core.Services.Interfaces;

namespace TextTableConverter.Core.Services
{
    public class FileService : IFileService
    {
        private readonly ITableService _tableService;

        public FileService(ITableService tableService)
        {
            _tableService = tableService;
        }

        public async Task<string> ReadFileAsync(string filePath)
        {
            if (!ValidateFilePath(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading file: {ex.Message}", ex);
            }
        }

        public async Task SaveFileAsync(string filePath, string content)
        {
            if (!ValidateFilePath(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            try
            {
                await File.WriteAllTextAsync(filePath, content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving file: {ex.Message}", ex);
            }
        }

        public bool ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                // Check if the path is valid
                var path = Path.GetFullPath(filePath);
                var dir = Path.GetDirectoryName(path);
                
                // Check if directory exists, create if it doesn't
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ReadPdfAsync(string filePath)
        {
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);
            var strategy = new LocationTextExtractionStrategy();
            var text = new StringBuilder();

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                text.AppendLine(PdfTextExtractor.GetTextFromPage(page, strategy));
            }

            return await Task.FromResult(text.ToString());
        }

        public async Task<string> ReadWordAsync(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var document = new XWPFDocument(stream);
            var text = new StringBuilder();

            foreach (var paragraph in document.Paragraphs)
            {
                text.AppendLine(paragraph.Text);
            }

            foreach (var table in document.Tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        text.Append(cell.GetText().Trim()).Append("\t");
                    }
                    text.AppendLine();
                }
            }

            return await Task.FromResult(text.ToString());
        }

        public async Task SaveCsvAsync(string filePath, ObservableCollection<object> data)
        {
            var stringBuilder = new StringBuilder();

            foreach (var row in data)
            {
                if (row is object[] cells)
                {
                    stringBuilder.AppendLine(string.Join(",", cells.Select(cell => $"\"{(cell?.ToString() ?? string.Empty).Replace("\"", "\"\"")}\""))); 
                }
            }

            await File.WriteAllTextAsync(filePath, stringBuilder.ToString(), Encoding.UTF8);
        }

        public async Task SaveExcelAsync(string filePath, ObservableCollection<object> data)
        {
            using var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");

            for (int i = 0; i < data.Count; i++)
            {
                var row = sheet.CreateRow(i);
                if (data[i] is object[] cells)
                {
                    for (int j = 0; j < cells.Length; j++)
                    {
                        var cell = row.CreateCell(j);
                        cell.SetCellValue(cells[j]?.ToString() ?? string.Empty);
                    }
                }
            }

            using var stream = File.Create(filePath);
            await Task.Run(() => workbook.Write(stream));
        }

        public async Task SaveTextAsync(string filePath, ObservableCollection<object> data)
        {
            var text = new StringBuilder();
            
            foreach (var row in data)
            {
                if (row is object[] cells)
                {
                    text.AppendLine(string.Join("\t", cells.Select(cell => cell?.ToString() ?? string.Empty)));
                }
            }

            await File.WriteAllTextAsync(filePath, text.ToString(), Encoding.UTF8);
        }

        public async Task ExportTableAsync(object data, string filePath, string format)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!ValidateFilePath(filePath))
            {
                throw new ArgumentException("Invalid file path", nameof(filePath));
            }

            format = format.ToLowerInvariant();
            switch (format)
            {
                case "csv":
                    await SaveCsvAsync(filePath, ConvertToCollection(data));
                    break;
                case "xlsx":
                    await SaveExcelAsync(filePath, ConvertToCollection(data));
                    break;
                case "txt":
                    await SaveTextAsync(filePath, ConvertToCollection(data));
                    break;
                default:
                    throw new NotSupportedException($"Format {format} is not supported for export.");
            }
        }

        public async Task ExportTableAsync(string filePath, string[][] tableData)
        {
            if (!ValidateFilePath(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            if (tableData == null || tableData.Length == 0)
                throw new ArgumentException("Table data cannot be empty", nameof(tableData));

            try
            {
                var extension = Path.GetExtension(filePath).ToLower();
                string content;

                switch (extension)
                {
                    case ".csv":
                        content = ConvertToCsv(tableData);
                        break;
                    case ".txt":
                        content = ConvertToText(tableData);
                        break;
                    default:
                        throw new ArgumentException("Unsupported file format", nameof(filePath));
                }

                await File.WriteAllTextAsync(filePath, content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting table: {ex.Message}", ex);
            }
        }

        private string ConvertToCsv(string[][] tableData)
        {
            var sb = new StringBuilder();
            foreach (var row in tableData)
            {
                var processedRow = row.Select(cell => $"\"{cell.Replace("\"", "\"\"")}\"");
                sb.AppendLine(string.Join(",", processedRow));
            }
            return sb.ToString();
        }

        private string ConvertToText(string[][] tableData)
        {
            var sb = new StringBuilder();
            foreach (var row in tableData)
            {
                sb.AppendLine(string.Join("\t", row));
            }
            return sb.ToString();
        }

        private ObservableCollection<object> ConvertToCollection(object data)
        {
            var collection = new ObservableCollection<object>();

            if (data is DataTable dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    collection.Add(row.ItemArray);
                }
            }
            else if (data is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    collection.Add(item);
                }
            }
            else
            {
                throw new ArgumentException("Unsupported data format", nameof(data));
            }

            return collection;
        }
    }
}
