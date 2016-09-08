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
        PlayList GetPlayList(string playlistId);
        ServiceResponse<List<PlayList>> GetPlayLists(string requestToken);
        ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest);
        ServiceResponse<List<PlayListItem>> SynchronizePlayListItems(string requestToken, string playlistId);

        void UpdatePlayLists(IEnumerable<PlayList> playLists);
    }
}
