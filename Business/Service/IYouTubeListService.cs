using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListService
    { 
        IEnumerable<PlayListItem> GetPlayListItems(string requestToken, string playListId);
        PlayList GetPlayList(string playListId);
        IEnumerable<PlayList> GetPlaylists(string requestToken);
        IEnumerable<VideoInfo> ShowSuggestions(string requestToken, string title);
    }
}
