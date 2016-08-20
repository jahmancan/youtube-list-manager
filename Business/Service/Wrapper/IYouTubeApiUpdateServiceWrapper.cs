using System;
using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public interface IYouTubeApiUpdateServiceWrapper
    {
        event EventHandler<UpdatePlayListItemEventArgs> PlaylistItemUpdated;
        event EventHandler<UpdatePlayListEventArgs> PlaylistUpdated;

        void UpdatePlayLists(IEnumerable<PlayList> playLists);
        void UpdatePlaylistItems(PlayList playList, IEnumerable<PlayListItem> playListItems);
        void DeletePlaylistItem(string playlistItemId);
    }
}