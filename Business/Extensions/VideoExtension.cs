using System;
using System.Text.RegularExpressions;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeListAPI.Business.Extensions
{
    public static class VideoExtension
    {
        private const string MinutesGroupName = "minutes";
        private const string SecondsGroupName = "seconds";

        private static readonly string namedDurationPattern =
            $@"PT(?<{MinutesGroupName}>\d+)M(?<{SecondsGroupName}>\d+)S";

        private static readonly Regex NamedDurationRegex = new Regex(namedDurationPattern, RegexOptions.IgnoreCase);

        internal static int GetDurationFromVideoInfo(this Video videoInfo)
        {
            var duration = videoInfo.ContentDetails.Duration;

            if (NamedDurationRegex.IsMatch(duration))
            {
                var match = NamedDurationRegex.Match(duration);
                var minutes = int.Parse(match.Groups[MinutesGroupName].Value);
                var seconds = int.Parse(match.Groups[SecondsGroupName].Value);
                return (int) new TimeSpan(0, minutes, seconds).TotalSeconds;
            }

            return 0;
        }
    }
}