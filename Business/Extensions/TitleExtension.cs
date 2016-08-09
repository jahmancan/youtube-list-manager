using System.Text.RegularExpressions;

namespace YouTubeListAPI.Business.Extensions
{
    internal static class TitleExtension
    {
        private const string TitleCleanerPattern = @"(\(|\[)[\w\s]+?(\)|\])";

        private const string MediaExtensionCleanerPattern = @"\.(mpg|mpeg|mp3|mp4|wma|avi|mov|mkv)";
        private static readonly Regex TitleCleaneRegex = new Regex(TitleCleanerPattern);

        private static readonly Regex MediaExtensionCleaneRegex = new Regex(MediaExtensionCleanerPattern,
            RegexOptions.IgnoreCase);

        internal static string CleanTitle(this string title)
        {
            return MediaExtensionCleaneRegex.Replace(TitleCleaneRegex.Replace(title, string.Empty), string.Empty);
        }
    }
}