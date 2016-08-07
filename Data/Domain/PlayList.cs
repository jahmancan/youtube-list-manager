using System.Collections;
using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class PlayList
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public PrivacyStatus PrivacyStatus { get; set; }

        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Track> Tracks { get; set; }

        public PlayList()
        {
            PrivacyStatus = PrivacyStatus.Public;
            Tracks = new List<Track>();
        }
    }
}
