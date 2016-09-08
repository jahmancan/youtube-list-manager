using System.Collections.Generic;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.CrossCutting.Extensions;

namespace YouTubeListManager.CrossCutting.Domain
{
    public class VideoInfo
    {
        public const string UnavailableVideoDescription = "This video is unavailable.";
        public const string PrivateVideoDescription = "This video is private.";
        public const string DeleteVideoTitle = "Deleted video";
        public const string PrivateVideoTitle = "Private video";
        public const string VideoKind = "youtube#video";

        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public bool Live { get; set; }
        public string ThumbnailUrl { get; set; }
        public PrivacyStatus PrivacyStatus { get; set; }

        public virtual ICollection<PlayListItem> PlayListItems { get; set; }

        public VideoInfo()
        {
            PlayListItems = new List<PlayListItem>();
        }

        public VideoInfo(Video video)
        {
            Hash = video.Id;
            Title = video.Snippet.Title;
            ThumbnailUrl = video.Snippet.Thumbnails.GetThumbnailUrl();
            Live = true;
            Duration = video.GetDurationFromVideoInfo();
        }
    }
}
