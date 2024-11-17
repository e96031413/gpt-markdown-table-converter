using System.Data;
using System.Collections.Generic;

namespace TextTableConverter.Core.Interfaces
{
    public interface ITableService
    {
        /// <summary>
        /// 將文本轉換為 DataTable
        /// </summary>
        DataTable ConvertToDataTable(string text, string delimiter = "\t");

        /// <summary>
        /// 將固定寬度文本轉換為 DataTable
        /// </summary>
        DataTable ConvertFixedWidthToDataTable(string text, int[] columnWidths);

        /// <summary>
        /// 將 DataTable 轉換為分隔符文本
        /// </summary>
        string ConvertToDelimitedText(DataTable table, string delimiter = "\t");

        /// <summary>
        /// 將 DataTable 轉換為固定寬度文本
        /// </summary>
        string ConvertToFixedWidthText(DataTable table, int[] columnWidths);

        /// <summary>
        /// 格式化 DataTable
        /// </summary>
        void FormatTable(DataTable table, IDictionary<string, string> formatSettings);
    }
}
