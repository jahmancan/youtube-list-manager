using System.Collections;
using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class YouTubeList
    {
        public int Id { get; set; }
        public string YouTubeListId { get; set; }
        public string Title { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<YouTubeTrack> YouTubeTracks { get; set; }

        public YouTubeList()
        {
            YouTubeTracks = new List<YouTubeTrack>();
        }
    }
}
