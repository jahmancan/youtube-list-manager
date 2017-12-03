using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Request;
using YouTubeListManager.CrossCutting.Response;

namespace YouTubeListManager.BusinessContracts.Service
{
    public interface IYouTubeListManagerService
    {
        Task<ServiceResponse<List<PlaylistItem>>> GetPlayListItemsAsync(string requestToken, string playlistId);
        ServiceResponse<List<PlaylistItem>> GetPlayListItems(string requestToken, string playlistId);

        Task<Playlist> GetPlayListAsync(string playlistId);
        Playlist GetPlayList(string playlistId);

        Task<ServiceResponse<List<Playlist>>> GetPlayListsAsync(string requestToken);
        ServiceResponse<List<Playlist>> GetPlayLists(string requestToken);
        
        Task<ServiceResponse<List<VideoInfo>>> SearchSuggestionsAsync(SearchRequest searchRequest);
        ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest);

        ServiceResponse<List<PlaylistItem>> SynchronizePlayListItems(string requestToken, string playlistId);

        void UpdatePlayLists(IEnumerable<Playlist> playLists);
    }
}
