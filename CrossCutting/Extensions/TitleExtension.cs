using System.Text.RegularExpressions;

namespace YouTubeListManager.CrossCutting.Extensions
{
    public static class TitleExtension
    {
        private const string TitleCleanerPattern = @"(\(|\[)[\w\s]+?(\)|\])";

        private const string MediaExtensionCleanerPattern = @"\.(mpg|mpeg|mp3|mp4|wma|avi|mov|mkv)";
        private static readonly Regex TitleCleaneRegex = new Regex(TitleCleanerPattern);

        private static readonly Regex MediaExtensionCleaneRegex = new Regex(MediaExtensionCleanerPattern,
            RegexOptions.IgnoreCase);

        public static string CleanTitle(this string title)
        {
            return MediaExtensionCleaneRegex.Replace(TitleCleaneRegex.Replace(title, string.Empty), string.Empty);
        }
    }
}