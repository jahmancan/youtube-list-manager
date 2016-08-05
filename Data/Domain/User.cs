using System.Collections;
using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<YouTubeList> YouTubeLists { get; set; }

        public User()
        {
            YouTubeLists = new List<YouTubeList>();
        }
    }
}
