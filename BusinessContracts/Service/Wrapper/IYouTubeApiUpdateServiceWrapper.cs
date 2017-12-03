using System;
using System.Collections.Generic;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.EventArgs;

namespace YouTubeListManager.BusinessContracts.Service.Wrapper
{
    public interface IYouTubeApiUpdateServiceWrapper
    {
        event EventHandler<UpdatePlayListItemEventArgs> PlaylistItemUpdated;
        event EventHandler<UpdatePlayListEventArgs> PlaylistUpdated;

        void UpdatePlaylists(IEnumerable<Playlist> playLists);
        void UpdatePlaylistItems(Playlist playList, IEnumerable<PlaylistItem> playListItems);
        void DeletePlaylistItem(string playlistItemId);
    }
}