using Microsoft.Extensions.Hosting.Internal;
using System.Text.RegularExpressions;

namespace SDS.Helpers
{
    public class Functions
    {
        public static string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Regular expression to match HTML tags
            return Regex.Replace(input, "<[^>]*>", string.Empty, RegexOptions.IgnoreCase);
        }

        public static string GetBase64Image(string fileName, string path)
        {
            var imagePath = Path.Combine(path, fileName);
            var data = $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(imagePath))}";
            return data;
        }

        public static string ConvertByteToBase64Image(byte[] imageBytes, string mimeType = "image/png")
        {
            return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
        }
    }
}