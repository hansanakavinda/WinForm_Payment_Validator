using OfficeOpenXml;
using Payment_Validator.OCR;
using System.Data;

namespace Payment_Validator
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel Files|*.xlsx;*.xls";
            dialog.Title = "Select Excel File";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialog.FileName;

                try
                {
                    DataTable dt = new DataTable();
                    dt = ReadExcel(filePath);

                    DisplayData(dt);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }

        }

        private DataTable ReadExcel(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Hansana"); // non commercial license
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

                MessageBox.Show($"Number of columns: {colCount}");

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

        private void DisplayData(DataTable dt)
        {
            dataView.DataSource = dt;

        }

        private void btnValidate_Click(object sender, EventArgs e)
        {   
            DataTable dt = (DataTable)dataView.DataSource;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No data to validate");
                return;
            }
            var readImages = new ReadImages();
            readImages.ValidatePayments(dt);
        }
    }
}
