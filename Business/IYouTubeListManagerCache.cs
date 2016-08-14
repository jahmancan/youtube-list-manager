using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business
{
    public interface IYouTubeListManagerCache
    {
        List<PlayList> PlayLists { get; }
        Dictionary<string, List<PlayListItem>> PlayListItems { get; }
        Dictionary<string, List<VideoInfo>> SearchList { get; }

        List<VideoInfo> GetSearchList(string title);
        List<PlayListItem> GetPlayListItems(string playListId);
    }
}