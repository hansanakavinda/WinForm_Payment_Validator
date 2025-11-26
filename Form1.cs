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

            // Set minimum width for all columns
            foreach (DataGridViewColumn column in dataView.Columns)
            {
                column.MinimumWidth = 100;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
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
            bool isValid = true;

            // Check NIC if column exists
            if (columns.Contains("NIC"))
            {
                string rowNIC = row["NIC"]?.ToString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(rowNIC) && !string.IsNullOrWhiteSpace(slipInfo.NIC))
                {
                    if (!rowNIC.Equals(slipInfo.NIC, StringComparison.OrdinalIgnoreCase))
                    {
                        isValid = false;
                    }
                }
            }

            // Check Full Name if column exists
            if (columns.Contains("Name") || columns.Contains("Full Name") || columns.Contains("FullName"))
            {
                string colName = columns.Contains("Name") ? "Name" : 
                                 columns.Contains("Full Name") ? "Full Name" : "FullName";
                string rowName = row[colName]?.ToString()?.Trim() ?? string.Empty;
                
                if (!string.IsNullOrWhiteSpace(rowName) && !string.IsNullOrWhiteSpace(slipInfo.FullName))
                {
                    // Partial match for names (case-insensitive)
                    if (!rowName.Contains(slipInfo.FullName, StringComparison.OrdinalIgnoreCase) &&
                        !slipInfo.FullName.Contains(rowName, StringComparison.OrdinalIgnoreCase))
                    {
                        isValid = false;
                    }
                }
            }

            // Check Deposit Reference if column exists
            if (columns.Contains("Reference") || columns.Contains("Deposit Reference") || columns.Contains("Ref"))
            {
                string colName = columns.Contains("Reference") ? "Reference" :
                                 columns.Contains("Deposit Reference") ? "Deposit Reference" : "Ref";
                string rowRef = row[colName]?.ToString()?.Trim() ?? string.Empty;
                
                if (!string.IsNullOrWhiteSpace(rowRef) && !string.IsNullOrWhiteSpace(slipInfo.DepositReference))
                {
                    if (!rowRef.Equals(slipInfo.DepositReference, StringComparison.OrdinalIgnoreCase))
                    {
                        isValid = false;
                    }
                }
            }

            // Check Bank Branch if column exists
            if (columns.Contains("Branch") || columns.Contains("Bank Branch"))
            {
                string colName = columns.Contains("Branch") ? "Branch" : "Bank Branch";
                string rowBranch = row[colName]?.ToString()?.Trim() ?? string.Empty;
                
                if (!string.IsNullOrWhiteSpace(rowBranch) && !string.IsNullOrWhiteSpace(slipInfo.BankBranch))
                {
                    if (!rowBranch.Contains(slipInfo.BankBranch, StringComparison.OrdinalIgnoreCase) &&
                        !slipInfo.BankBranch.Contains(rowBranch, StringComparison.OrdinalIgnoreCase))
                    {
                        isValid = false;
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
                    if (!slipInfo.Timestamp.Contains(rowDate) && !rowDate.Contains(slipInfo.Timestamp))
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
    }
}
