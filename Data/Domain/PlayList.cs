using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class PlayList
    {
        public PlayList()
        {
            PrivacyStatus = PrivacyStatus.Public;
            PlayListItems = new List<PlayListItem>();
        }

        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public PrivacyStatus PrivacyStatus { get; set; }
        public string ThumbnailUrl { get; set; }
        public long? ItemCount { get; set; }


        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<PlayListItem> PlayListItems { get; set; }
    }
}