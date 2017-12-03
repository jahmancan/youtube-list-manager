using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.CrossCutting.EventArgs
{
    public class UpdatePlayListEventArgs : System.EventArgs
    {
        public UpdatePlayListEventArgs(Playlist playList)
        {
            PlayList = playList;
        }

        public Playlist PlayList { get; private set; }
    }

    public class UpdatePlayListItemEventArgs : System.EventArgs
    {
        public UpdatePlayListItemEventArgs(Playlist playList, PlaylistItem playListItem)
        {
            PlayList = playList;
            PlayListItem = playListItem;
        }

        public Playlist PlayList { get; private set; }
        public PlaylistItem PlayListItem { get; private set; }
    }
}