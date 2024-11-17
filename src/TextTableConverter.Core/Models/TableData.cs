using System;
using System.Collections.Generic;

namespace TextTableConverter.Core.Models
{
    public class TableData
    {
        /// <summary>
        /// 表格標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 欄位名稱列表
        /// </summary>
        public List<string> Columns { get; set; }

        /// <summary>
        /// 表格數據（不包含標題行）
        /// </summary>
        public List<List<string>> Rows { get; set; }

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最後修改時間
        /// </summary>
        public DateTime LastModifiedAt { get; set; }

        public TableData()
        {
            Title = string.Empty;
            Columns = new List<string>();
            Rows = new List<List<string>>();
            CreatedAt = DateTime.Now;
            LastModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// 添加新行
        /// </summary>
        /// <param name="row">行數據</param>
        public void AddRow(List<string> row)
        {
            if (row.Count != Columns.Count)
            {
                throw new ArgumentException("行數據的列數必須與表格列數相同");
            }

            Rows.Add(row);
            LastModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// 添加新列
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默認值</param>
        public void AddColumn(string columnName, string defaultValue = "")
        {
            Columns.Add(columnName);
            foreach (var row in Rows)
            {
                row.Add(defaultValue);
            }
            LastModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// 轉換為二維數組格式
        /// </summary>
        /// <returns>包含標題行的二維數組</returns>
        public string[][] ToArray()
        {
            var result = new List<string[]>();
            result.Add(Columns.ToArray());
            
            foreach (var row in Rows)
            {
                result.Add(row.ToArray());
            }

            return result.ToArray();
        }

        /// <summary>
        /// 從二維數組創建表格數據
        /// </summary>
        /// <param name="data">包含標題行的二維數組</param>
        /// <returns>表格數據對象</returns>
        public static TableData FromArray(string[][] data)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("數據不能為空");
            }

            var table = new TableData
            {
                Columns = new List<string>(data[0]),
                Rows = new List<List<string>>()
            };

            for (int i = 1; i < data.Length; i++)
            {
                table.Rows.Add(new List<string>(data[i]));
            }

            return table;
        }
    }
}
