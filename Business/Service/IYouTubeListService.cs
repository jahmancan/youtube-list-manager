using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListService
    {
        IEnumerable<PlayListItem> GetPlayListItems(string playListId);
        PlayList GetPlayList(string playListId);
        IEnumerable<PlayList> GetPlaylists();
        IEnumerable<VideoInfo> ShowSuggestions(string title);
    }
}
