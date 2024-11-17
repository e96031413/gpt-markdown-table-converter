using System.Data;

namespace TextTableConverter.Core.Services.Interfaces
{
    public interface ITableService
    {
        DataTable ConvertTextToTable(string text);
        string ConvertTableToText(DataTable table);
        string AnalyzeTableStructure(DataTable table);
    }
}
