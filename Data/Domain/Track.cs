using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class Track
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public bool Live { get; set; }
        public string ThumbnailUrl { get; set; }

        public virtual ICollection<PlayList> PlayLists { get; set; }

        public Track()
        {
            PlayLists = new List<PlayList>();
        }
    }
}
