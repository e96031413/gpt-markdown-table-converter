using System;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using TextTableConverter.Core.Services.Interfaces;

namespace TextTableConverter.Core.Services
{
    public class TableService : ITableService
    {
        private readonly ITextAnalysisService _textAnalysisService;

        public TableService(ITextAnalysisService textAnalysisService)
        {
            _textAnalysisService = textAnalysisService ?? throw new ArgumentNullException(nameof(textAnalysisService));
        }

        public DataTable ConvertTextToTable(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty or whitespace.", nameof(text));

            var table = new DataTable();
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length == 0)
                return table;

            // Process header
            var headers = lines[0].Split('\t');
            foreach (var header in headers)
            {
                table.Columns.Add(header?.Trim() ?? $"Column{table.Columns.Count + 1}");
            }

            // Process data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                var row = table.NewRow();
                
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    row[j] = j < values.Length ? values[j] : DBNull.Value;
                }
                
                table.Rows.Add(row);
            }

            return table;
        }

        public string ConvertTableToText(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var sb = new StringBuilder();

            // Add headers
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0) sb.Append('\t');
                sb.Append(table.Columns[i].ColumnName);
            }
            sb.AppendLine();

            // Add rows
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i > 0) sb.Append('\t');
                    var value = row[i];
                    sb.Append(value == DBNull.Value ? "" : value.ToString());
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string AnalyzeTableStructure(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var analysis = new StringBuilder();
            analysis.AppendLine($"Table Analysis:");
            analysis.AppendLine($"- Number of columns: {table.Columns.Count}");
            analysis.AppendLine($"- Number of rows: {table.Rows.Count}");
            analysis.AppendLine("\nColumn Details:");

            foreach (DataColumn column in table.Columns)
            {
                analysis.AppendLine($"\nColumn: {column.ColumnName}");
                analysis.AppendLine($"- Data Type: {column.DataType.Name}");
                analysis.AppendLine($"- Allows Null: {column.AllowDBNull}");

                // Analyze sample data
                var distinctValues = new HashSet<string>();
                var nonNullCount = 0;
                var numericCount = 0;
                var dateCount = 0;

                foreach (DataRow row in table.Rows)
                {
                    var value = row[column];
                    if (value != DBNull.Value && value != null)
                    {
                        nonNullCount++;
                        var strValue = value.ToString() ?? string.Empty;
                        distinctValues.Add(strValue);

                        if (decimal.TryParse(strValue, out _))
                            numericCount++;
                        if (DateTime.TryParse(strValue, out _))
                            dateCount++;
                    }
                }

                var nonNullPercentage = table.Rows.Count > 0 
                    ? (nonNullCount * 100.0 / table.Rows.Count) 
                    : 0;
                analysis.AppendLine($"- Non-null values: {nonNullPercentage:F1}%");
                analysis.AppendLine($"- Distinct values: {distinctValues.Count}");

                if (nonNullCount > 0)
                {
                    var potentialType = "Text";
                    if (numericCount == nonNullCount)
                        potentialType = "Numeric";
                    else if (dateCount == nonNullCount)
                        potentialType = "Date";
                    analysis.AppendLine($"- Suggested Type: {potentialType}");
                }
            }

            return analysis.ToString();
        }

        private string PadToWidth(string? text, int width)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', width);

            if (text.Length > width)
                return text.Substring(0, width);

            return text.PadRight(width);
        }

        private void ConvertColumnToType(DataTable table, string columnName, Type targetType)
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(columnName);
            ArgumentNullException.ThrowIfNull(targetType);

            if (!table.Columns.Contains(columnName))
                return;

            var column = table.Columns[columnName] ?? throw new InvalidOperationException($"Column {columnName} not found in table.");
            if (column.DataType == targetType)
                return;

            var newColumn = new DataColumn(columnName + "_new", targetType);
            table.Columns.Add(newColumn);

            foreach (DataRow row in table.Rows)
            {
                try
                {
                    var value = row[column];
                    if (value is not null and not DBNull)
                    {
                        row[newColumn] = Convert.ChangeType(value, targetType);
                    }
                    else
                    {
                        row[newColumn] = DBNull.Value;
                    }
                }
                catch
                {
                    row[newColumn] = DBNull.Value;
                }
            }

            table.Columns.Remove(column);
            newColumn.ColumnName = columnName;
        }
    }
}
