using System.Text.RegularExpressions;

namespace NihongoAcademy.Helpers
{
    /// <summary>
    /// Safely truncates HTML content by stripping tags first, then truncating plain text.
    /// Avoids breaking HTML tags mid-tag.
    /// </summary>
    public static class TextHelper
    {
        /// <summary>
        /// Renders learner-authored plain text for display: HTML-encodes everything,
        /// then turns newlines into &lt;br/&gt;. Encoding first guarantees no user-supplied
        /// tag can survive, because the only tags present are the ones we add here.
        /// </summary>
        public static string ToSafeHtmlLines(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            return System.Net.WebUtility.HtmlEncode(normalized).Replace("\n", "<br/>");
        }

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
