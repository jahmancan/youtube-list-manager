using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.EventArgs;

namespace YouTubeListManager.BusinessContracts.Service.Wrapper
{
    public interface IYouTubeApiUpdateServiceWrapper
    {
        event EventHandler<UpdatePlaylistItemEventArgs> PlaylistItemUpdated;
        event EventHandler<UpdatePlaylistEventArgs> PlaylistUpdated;

        void UpdatePlaylists(IEnumerable<Playlist> playLists);
        void UpdatePlaylistItems(Playlist playlist, IEnumerable<PlaylistItem> playListItems);
        Task DeletePlaylistItem(string playlistItemId);
    }
}