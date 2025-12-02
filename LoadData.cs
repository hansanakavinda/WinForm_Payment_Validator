using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment_Validator
{
    public class LoadData
    {
        public DataTable ReadExcel(string filePath)
        {

            DataTable dt = new DataTable();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets[0]; // first worksheet

                int rowCount = ws.Dimension.Rows;
                int colCount = 0;
                for (int col = 1; col <= ws.Dimension.Columns; col++)
                {
                    if (ws.Cells[1, col].Value != null &&
                        !string.IsNullOrWhiteSpace(ws.Cells[1, col].Value.ToString()))
                    {
                        colCount = col;
                    }
                }

                // add headers to datatable
                for (int col = 1; col <= colCount; col++)
                {
                    string colName = ws.Cells[1, col].Value?.ToString() ?? $"Column{col}";
                    dt.Columns.Add(colName);

                }

                // add data to rows
                for (int row = 2; row <= rowCount; row++)
                {
                    DataRow dr = dt.NewRow();
                    for (int col = 1; col <= colCount; col++)
                    {
                        dr[col - 1] = ws.Cells[row, col].Value?.ToString() ?? string.Empty;
                    }
                    dt.Rows.Add(dr);
                }

            }
            return dt;
        }
    }
}
