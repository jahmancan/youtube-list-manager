using System.Collections;
using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class PlayList
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Track> Tracks { get; set; }

        public PlayList()
        {
            Tracks = new List<Track>();
        }
    }
}
