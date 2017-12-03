using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class Playlist
    {
        public Playlist()
        {
            PrivacyStatus = PrivacyStatus.Public;
            PlaylistItems = new List<PlaylistItem>();
        }

        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public PrivacyStatus PrivacyStatus { get; set; }
        public string ThumbnailUrl { get; set; }
        public long? ItemCount { get; set; }
        public string PlaylistItemsNextPageToken { get; set; }


        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<PlaylistItem> PlaylistItems { get; set; }
    }
}