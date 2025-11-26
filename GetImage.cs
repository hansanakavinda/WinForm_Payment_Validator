namespace Payment_Validator
{
    public class GetImage
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<Image?> GetImageFromDrive(string link)
        {
            try
            {
                string directLink = ConvertToDirectLink(link);

                var bytes = await client.GetByteArrayAsync(directLink);

                using (var ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ConvertToDirectLink(string link)
        {
            if (link.Contains("file/d/"))
            {
                var parts = link.Split(new[] { "file/d/" }, StringSplitOptions.None);
                var part2 = parts[1];
                var fileId = part2.Split('/')[0];

                return $"https://drive.google.com/uc?export=download&id={fileId}";
            }

            if (link.Contains("open?id="))
            {
                var fileId = link.Split(new[] { "open?id=" }, StringSplitOptions.None);
                return $"https://drive.google.com/uc?export=download&id={fileId}";
            }

            if (link.Contains("uc?export=download"))
            {
                return link;
            }

            throw new Exception("Unsupported google drive link format");
        }
    }
}
