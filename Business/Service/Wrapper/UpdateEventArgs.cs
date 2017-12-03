using System;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public class UpdatePlayListEventArgs : EventArgs
    {
        public UpdatePlayListEventArgs(Playlist playlist)
        {
            Playlist = playlist;
        }

        public Playlist Playlist { get; private set; }
    }

    public class UpdatePlayListItemEventArgs : EventArgs
    {
        public UpdatePlayListItemEventArgs(Playlist playlist, PlaylistItem playlistItem)
        {
            Playlist = playlist;
            PlaylistItem = playlistItem;
        }

        public Playlist Playlist { get; private set; }
        public PlaylistItem PlaylistItem { get; private set; }
    }
}