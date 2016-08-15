using System;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public class UpdatePlayListEventArgs : EventArgs
    {
        public UpdatePlayListEventArgs(PlayList playList)
        {
            PlayList = playList;
        }

        public PlayList PlayList { get; private set; }
    }

    public class UpdatePlayListItemEventArgs : EventArgs
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