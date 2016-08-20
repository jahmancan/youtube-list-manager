using System.Collections.Generic;
using Google.Apis.YouTube.v3;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListManagerService
    {
        ServiceResponse<List<PlayListItem>> GetPlayListItems(string requestToken, string playListId);
        PlayList GetPlayList(string playListId);
        ServiceResponse<List<PlayList>> GetPlaylists(string requestToken);
        ServiceResponse<List<VideoInfo>> ShowSuggestions(string requestToken, string title, SearchResource.ListRequest.VideoDurationEnum videoDuration = default(SearchResource.ListRequest.VideoDurationEnum));

        void UpdatePlayLists(IEnumerable<PlayList> playLists);
    }
}
