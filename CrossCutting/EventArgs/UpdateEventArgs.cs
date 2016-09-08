using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.CrossCutting.EventArgs
{
    public class UpdatePlayListEventArgs : System.EventArgs
    {
        public UpdatePlayListEventArgs(PlayList playList)
        {
            PlayList = playList;
        }

        public PlayList PlayList { get; private set; }
    }

    public class UpdatePlayListItemEventArgs : System.EventArgs
    {
        public UpdatePlayListItemEventArgs(PlayList playList, PlayListItem playListItem)
        {
            PlayList = playList;
            PlayListItem = playListItem;
        }

        public PlayList PlayList { get; private set; }
        public PlayListItem PlayListItem { get; private set; }
    }
}