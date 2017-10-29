using System;
using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public interface IYouTubeApiUpdateServiceWrapper
    {
        event EventHandler<UpdatePlayListItemEventArgs> PlaylistItemUpdated;
        event EventHandler<UpdatePlayListEventArgs> PlaylistUpdated;

        void UpdatePlayLists(IEnumerable<Playlist> playLists);
        void UpdatePlaylistItems(Playlist playlist, IEnumerable<PlaylistItem> playListItems);
        void DeletePlaylistItem(string playlistItemId);
    }
}