using System.Collections.Generic;

namespace YouTubeListManager.CrossCutting.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Playlist> PlayLists { get; set; }

        public User()
        {
            PlayLists = new List<Playlist>();
        }
    }
}
