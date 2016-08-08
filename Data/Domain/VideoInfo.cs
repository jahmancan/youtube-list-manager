using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeListManager.Data.Domain
{
    public class VideoInfo
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public bool Live { get; set; }
        public string ThumbnailUrl { get; set; }

        public virtual ICollection<PlayListItem> PlayListItems { get; set; }

        public VideoInfo()
        {
            PlayListItems = new List<PlayListItem>();
        }
    }
}
