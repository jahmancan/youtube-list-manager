﻿using System.Collections.Generic;

namespace YouTubeListManager.CrossCutting.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Playlist> Playlists { get; set; }

        public User()
        {
            Playlists = new List<Playlist>();
        }
    }
}
