using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListService
    {
        IEnumerable<PlayListItem> GetPlayListItems(string playListId, bool withVideoInfo = true);
        IEnumerable<PlayList> GetPlaylists(string playListId = default(string), bool withPlayListItems = false);
        IEnumerable<VideoInfo> ShowSuggestions(string title);
    }
}
