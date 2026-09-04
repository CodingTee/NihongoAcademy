using System.Text.RegularExpressions;

namespace NihongoAcademy.Helpers
{
    /// <summary>
    /// Safely truncates HTML content by stripping tags first, then truncating plain text.
    /// Avoids breaking HTML tags mid-tag.
    /// </summary>
    public static class TextHelper
    {
        public static string TruncateHtml(string? html, int maxLength)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Strip HTML tags to get plain text
            var plainText = Regex.Replace(html, "<[^>]*>", string.Empty);

            // Decode HTML entities
            plainText = System.Net.WebUtility.HtmlDecode(plainText);

            if (plainText.Length <= maxLength) return plainText;

            // Find the last space before maxLength to avoid cutting mid-word
            var truncated = plainText.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');
            if (lastSpace > maxLength / 2)
            {
                truncated = truncated.Substring(0, lastSpace);
            }

            return truncated + "...";
        }
    }
}
