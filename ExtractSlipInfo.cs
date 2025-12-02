using System;
using System.Text.RegularExpressions;

namespace Payment_Validator
{
    public class ExtractSlipInfo
    {
        public class SlipInfo
        {
            public string? NIC { get; set; }
            public string? Timestamp { get; set; }
        }

        public SlipInfo ExtractInfo(string ocrText)
        {
            var info = new SlipInfo();

            // Normalize the text
            string text = ocrText.Replace("\n", " ").Replace("\r", " ").Trim();

            // 1. Extract NIC (Sri Lankan NIC old or new format)
            var nicMatch = Regex.Match(text, @"\b\d{12}\b|\b\d{9}[VvXx]\b");
            info.NIC = nicMatch.Success ? nicMatch.Value : null;

            // 2. Extract Timestamp (supports many formats)
            var timeMatch = Regex.Match(text,
                @"\b\d{2}[/\-\.]\d{2}[/\-\.]\d{2}\b|" +                                      // DD/MM/YY or YY/MM/DD (e.g., 20/07/25)
                @"\b(20\d{2}[-/\.]\d{1,2}[-/\.]\d{1,2}\s+\d{1,2}:\d{2}(:\d{2})?)\b|" +      // YYYY-MM-DD HH:MM
                @"\b(\d{1,2}[-/\.]\d{1,2}[-/\.](20\d{2})\s+\d{1,2}:\d{2}(:\d{2})?)\b|" +   // DD-MM-YYYY HH:MM
                @"\b(20\d{2}[-/\.]\d{1,2}[-/\.]\d{1,2})\b"                                  // YYYY-MM-DD
            );
            if (timeMatch.Success)
            {
                string extractedDate = timeMatch.Value;

                // Convert short format (20/07/25) to full format (20/07/2025)
                if (Regex.IsMatch(extractedDate, @"^\d{2}[/\-\.]\d{2}[/\-\.]\d{2}$"))
                {
                    extractedDate = ConvertShortDateToFullDate(extractedDate);
                }

                info.Timestamp = extractedDate;
            }
            else
            {
                info.Timestamp = null;
            }

            MessageBox.Show($"text: {text}\n\n, Extracted NIC: {info.NIC}, Timestamp: {info.Timestamp}");
            return info;
        }

        private string ConvertShortDateToFullDate(string shortDate)
        {
            var parts = shortDate.Split(new[] { '/', '-', '.' });

            if (parts.Length == 3)
            {
                string day = parts[0];
                string month = parts[1];
                string year = "20" + parts[2]; // Convert YY to 20YY

                return $"{month}/{day}/{year}"; // DD/MM/YYYY
            }

            return shortDate;
        }
    }
}
