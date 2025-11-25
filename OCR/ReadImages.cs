using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment_Validator.OCR
{
    public class ReadImages
    {
        public static void ValidatePayments(DataTable dt)
        {
            int slipColIndex = 0;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dt.Columns[i].ColumnName.Trim().Equals("Slip"))
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

            DataRow row = dt.Rows[0];
            string link = row[slipColIndex]?.ToString() ?? $"No link found";

            MessageBox.Show($"Link from 'Slip' : {link}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }

        private void GetImageFromDrive(string link)
        {
            
        }
    }

    
}
