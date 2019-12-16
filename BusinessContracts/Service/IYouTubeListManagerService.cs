using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Request;
using YouTubeListManager.CrossCutting.Response;

namespace YouTubeListManager.BusinessContracts.Service
{
    public interface IYouTubeListManagerService
    {
        Task<ServiceResponse<List<PlaylistItem>>> GetPlaylistItemsAsync(string requestToken, string playlistId, bool onlyOfflineUsage = false);
        //ServiceResponse<List<PlaylistItem>> GetPlaylistItems(string requestToken, string playlistId);

        Task<Playlist> GetPlaylistAsync(string playlistId, bool onlyOfflineUsage = false, bool withPlaylistItems = true);
       // Playlist GetPlaylist(string playlistId);

        Task<ServiceResponse<List<Playlist>>> GetPlaylistsAsync(string requestToken, bool onlyOfflineUsage = false);
        //ServiceResponse<List<Playlist>> GetPlaylists(string requestToken);
        
        Task<ServiceResponse<List<VideoInfo>>> SearchSuggestionsAsync(SearchRequest searchRequest);
        //ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest);

        ServiceResponse<List<PlaylistItem>> SynchronizePlaylistItems(string requestToken, string playlistId);

        void UpdatePlaylists(IEnumerable<Playlist> playLists);
    }
}
