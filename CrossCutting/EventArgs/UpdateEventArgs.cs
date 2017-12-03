using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.CrossCutting.EventArgs
{
    public class UpdatePlaylistEventArgs : System.EventArgs
    {
        public UpdatePlaylistEventArgs(Playlist playlist)
        {
            Playlist = playlist;
        }

        public Playlist Playlist { get; private set; }
    }

    public class UpdatePlaylistItemEventArgs : System.EventArgs
    {
        public UpdatePlaylistItemEventArgs(Playlist playlist, PlaylistItem playlistItem)
        {
            Playlist = playlist;
            PlaylistItem = playlistItem;
        }

        public Playlist Playlist { get; private set; }
        public PlaylistItem PlaylistItem { get; private set; }
    }
}