using System.Collections;
using System.Collections.Generic;

namespace YouTubeListManager.Data.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<PlayList> PlayLists { get; set; }

        public User()
        {
            PlayLists = new List<PlayList>();
        }
    }
}
