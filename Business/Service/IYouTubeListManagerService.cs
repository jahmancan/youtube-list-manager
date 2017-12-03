using System.Collections.Generic;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListManagerService
    {
        ServiceResponse<List<PlaylistItem>> GetPlaylistItems(string requestToken, string playListId);
        Playlist GetPlayList(string playListId);
        ServiceResponse<List<Playlist>> GetPlaylists(string requestToken);
        ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest);

        void UpdatePlayLists(IEnumerable<Playlist> playLists);
    }
}
