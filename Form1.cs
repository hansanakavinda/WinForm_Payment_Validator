using OfficeOpenXml;
using Payment_Validator.OCR;
using System.Data;

namespace Payment_Validator
{
    public partial class mainForm : Form
    {

        GetImage getImage = new GetImage();
        ReadImages readImages = new ReadImages(@"tessdata");
        ExtractSlipInfo extractSlipInfo = new ExtractSlipInfo();
        
        public mainForm()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("Hansana"); // non commercial license
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

        private void DisplayData(DataTable dt)
        {
            dataView.DataSource = dt;

            // Set minimum width for all columns
            foreach (DataGridViewColumn column in dataView.Columns)
            {
                column.MinimumWidth = 100;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            // Format the 2nd column (index 1) as date if it exists
            if (dataView.Columns.Count >= 2)
            {
                dataView.Columns[1].DefaultCellStyle.Format = "d/MM/yyyy";
                
                // If values are still showing as numbers, convert them
                foreach (DataGridViewRow row in dataView.Rows)
                {
                    if (row.Cells[1].Value != null)
                    {
                        string cellValue = row.Cells[1].Value.ToString();
                        if (double.TryParse(cellValue, out double numericValue))
                        {
                            try
                            {
                                DateTime date = DateTime.FromOADate(numericValue);
                                row.Cells[1].Value = date.ToString("MM/dd/yyyy");
                            }
                            catch
                            {
                                // Keep original value if conversion fails
                            }
                        }
                    }
                }
            }
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {   
            try 
            {
                DataTable dt = (DataTable)dataView.DataSource;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No data to validate");
                    return;
                }
                
                ValidatePayments(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            
        }

        private async void ValidatePayments(DataTable dt)
        {
            // Find the Slip column index
            int slipColIndex = -1;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dt.Columns[i].ColumnName.Trim().Equals("Slip", StringComparison.OrdinalIgnoreCase))
                {
                    slipColIndex = i;
                    break;
                }
            }

            if (slipColIndex == -1)
            {
                MessageBox.Show("Slip column not found in the data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Add Validation Status column if it doesn't exist
            if (!dt.Columns.Contains("Validation Status"))
            {
                dt.Columns.Add("Validation Status", typeof(string));
                
                // Set minimum width for the new column
                int colIndex = dt.Columns["Validation Status"]!.Ordinal;
                if (dataView.Columns.Count > colIndex)
                {
                    dataView.Columns[colIndex].MinimumWidth = 150;
                    dataView.Columns[colIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }

            int validationColIndex = dt.Columns["Validation Status"]!.Ordinal;

            // Disable UI during validation
            btnValidate.Enabled = false;
            btnUpload.Enabled = false;
            btnValidate.Text = "Processing...";

            int validCount = 0;
            int invalidCount = 0;

            // Iterate through all rows
            for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
            {
                DataRow row = dt.Rows[rowIndex];
                
                try
                {
                    // Get the slip link
                    string link = row[slipColIndex]?.ToString()?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(link))
                    {
                        row[validationColIndex] = "Error: No slip link provided";
                        invalidCount++;
                        continue;
                    }

                    // Download image from Google Drive
                    Image? img = await getImage.GetImageFromDrive(link);

                    if (img == null)
                    {
                        row[validationColIndex] = "Error: Failed to download image";
                        invalidCount++;
                        continue;
                    }

                    // Extract text using OCR
                    string text = readImages.ExtractTextFromImage(img);
                    
                    img.Dispose();

                    // Check if OCR extraction failed
                    if (text.StartsWith("[OCR ERROR]"))
                    {
                        row[validationColIndex] = $"Error: {text}";
                        MessageBox.Show($"{text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        invalidCount++;
                        continue;
                    }

                    // Extract slip information
                    ExtractSlipInfo.SlipInfo slipInfo = extractSlipInfo.ExtractInfo(text);

                    // Validate extracted data against row data
                    bool isValid = ValidateSlipData(row, slipInfo, dt.Columns);

                    if (isValid)
                    {
                        row[validationColIndex] = "Valid";
                        validCount++;
                    }
                    else
                    {
                        row[validationColIndex] = "Invalid: Data mismatch";
                        invalidCount++;
                    }
                }
                catch (Exception ex)
                {
                    row[validationColIndex] = $"Error: {ex.Message}";
                    invalidCount++;
                }

                // Refresh the DataGridView to show progress
                dataView.Refresh();
            }

            // Re-enable UI
            btnValidate.Enabled = true;
            btnUpload.Enabled = true;
            btnValidate.Text = "Validate Payments";

            // Show summary
            MessageBox.Show($"Validation Complete!\n\nValid: {validCount}\nInvalid/Error: {invalidCount}",
                "Validation Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool ValidateSlipData(DataRow row, ExtractSlipInfo.SlipInfo slipInfo, DataColumnCollection columns)
        {
            
            bool nicValid = false;
            bool dateValid = false;

            // Check NIC if column exists
            if (columns.Contains("National Identity Card No"))
            {
                string rowNIC = row["National Identity Card No"]?.ToString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(rowNIC) && !string.IsNullOrWhiteSpace(slipInfo.NIC))
                {
                    if (rowNIC.Equals(slipInfo.NIC, StringComparison.OrdinalIgnoreCase))
                    {
                        nicValid = true;
                    }
                }
            }

            // Check Timestamp if column exists
            if (columns.Contains("Date") || columns.Contains("Timestamp") || columns.Contains("DateTime"))
            {
                string colName = columns.Contains("Date") ? "Date" :
                                 columns.Contains("Timestamp") ? "Timestamp" : "DateTime";
                string rowDate = row[colName]?.ToString()?.Trim() ?? string.Empty;
                
                if (!string.IsNullOrWhiteSpace(rowDate) && !string.IsNullOrWhiteSpace(slipInfo.Timestamp))
                {
                    // Basic date comparison (can be enhanced)
                    if (slipInfo.Timestamp.Contains(rowDate) || rowDate.Contains(slipInfo.Timestamp))
                    {
                        dateValid = true;
                    }
                }
            }
            MessageBox.Show($"NIC Valid: {nicValid}, Date Valid: {dateValid}");

            return nicValid && dateValid;
        }
    }
}
