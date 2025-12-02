using Tesseract;

namespace Payment_Validator.OCR
{
    public class ReadImages
    {

        private readonly string tessDataPath;

        public ReadImages(string tessDataFolderPath)
        {
            tessDataPath = tessDataFolderPath;
        }

        public string ExtractTextFromImage(Image img)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(img))
                using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                using (var pix = Pix.LoadFromMemory(ImageToByteArray(bitmap)))
                using (var page = engine.Process(pix))
                {
                    return page.GetText();

                }
            }
            catch (Exception ex)
            {
                return $"[OCR ERROR] {ex.Message}";
            }
        }

        private static byte[] ImageToByteArray(Image image)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                // Use Jpeg format explicitly for JPG images
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

    }
}
