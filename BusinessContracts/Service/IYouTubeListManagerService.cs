using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Request;
using YouTubeListManager.CrossCutting.Response;

namespace YouTubeListManager.BusinessContracts.Service
{
    public interface IYouTubeListManagerService
    {
        Task<ServiceResponse<List<PlayListItem>>> GetPlayListItemsAsync(string requestToken, string playlistId);
        ServiceResponse<List<PlayListItem>> GetPlayListItems(string requestToken, string playlistId);

        Task<PlayList> GetPlayListAsync(string playlistId);
        PlayList GetPlayList(string playlistId);

        Task<ServiceResponse<List<PlayList>>> GetPlayListsAsync(string requestToken);
        ServiceResponse<List<PlayList>> GetPlayLists(string requestToken);
        
        Task<ServiceResponse<List<VideoInfo>>> SearchSuggestionsAsync(SearchRequest searchRequest);
        ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest);

        ServiceResponse<List<PlayListItem>> SynchronizePlayListItems(string requestToken, string playlistId);

        void UpdatePlayLists(IEnumerable<PlayList> playLists);
    }
}
