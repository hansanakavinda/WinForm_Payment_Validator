using System;
using System.Text.RegularExpressions;

namespace Payment_Validator
{
    public class ExtractSlipInfo
    {
        public class SlipInfo
        {
            public string? FullName { get; set; }
            public string? NIC { get; set; }
            public string? Timestamp { get; set; }
            public string? DepositReference { get; set; }
            public string? BankBranch { get; set; }
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
                @"\b(20\d{2}[-/\.]\d{1,2}[-/\.]\d{1,2}\s+\d{1,2}:\d{2}(:\d{2})?)\b|" +        // YYYY-MM-DD HH:MM
                @"\b(\d{1,2}[-/\.]\d{1,2}[-/\.](20\d{2})\s+\d{1,2}:\d{2}(:\d{2})?)\b|" +     // DD-MM-YYYY HH:MM
                @"\b(20\d{2}[-/\.]\d{1,2}[-/\.]\d{1,2})\b"                                  // YYYY-MM-DD
            );
            info.Timestamp = timeMatch.Success ? timeMatch.Value : null;

            // 3. Extract deposit reference
            // Typical patterns: ABC12345, DEP-123456, SLIP NO 123456
            var refMatch = Regex.Match(text,
                @"(Ref(erence)?\.?\s*[:\-]?\s*[A-Za-z0-9\-]{5,})|" +
                @"(Slip\s*No\.?\s*[A-Za-z0-9\-]{3,})|" +
                @"\b([A-Z]{2,5}\-?\d{4,10})\b"
            );
            info.DepositReference = refMatch.Success ? Clean(refMatch.Value) : null;

            // 4. Extract Full Name
            // Heuristic: Names are often OCR’d after keywords like:
            // Name:, Account Name:, Deposited by:
            var nameMatch = Regex.Match(text,
                @"(?i)(Name|Account\s*Name|Deposited\s*By)\s*[:\-]?\s*([A-Za-z\. ]{3,})"
            );
            if (nameMatch.Success)
                info.FullName = nameMatch.Groups[2].Value.Trim();

            // If no keyword found, fallback by detecting 2–4 capitalized words
            if (info.FullName == null)
            {
                var fallback = Regex.Match(text, @"([A-Z][a-z]+(?:\s+[A-Z][a-z]+){1,3})");
                info.FullName = fallback.Success ? fallback.Value : null;
            }

            // 5. Extract bank branch
            // OCR patterns like: "Branch: Galle", "WELIGAMA BRANCH", "MATARA"
            var branchMatch = Regex.Match(text,
                @"(?i)(Branch|BRANCH)\s*[:\-]?\s*([A-Za-z ]{3,})"
            );
            if (branchMatch.Success)
                info.BankBranch = branchMatch.Groups[2].Value.Trim();
            else
            {
                // fallback: detect typical SL branch names (optional)
                var knownBranch = Regex.Match(text,
                    @"\b(Galle|Matara|Weligama|Colombo|Kandy|Kurunegala|Gampaha|Jaffna|Negombo|Kalutara|Badulla|Ratnapura)\b",
                    RegexOptions.IgnoreCase);
                info.BankBranch = knownBranch.Success ? knownBranch.Value : null;
            }

            return info;
        }

        private string Clean(string s)
        {
            return s.Replace("Ref", "")
                    .Replace("ref", "")
                    .Replace("Reference", "")
                    .Replace(":", "")
                    .Replace("Slip No", "")
                    .Trim();
        }
    }
}
