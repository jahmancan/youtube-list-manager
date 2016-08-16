using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business
{
    public class YouTubeListManagerCache : IYouTubeListManagerCache
    {
        public List<PlayList> PlayLists { get; }
        public Dictionary<string, List<PlayListItem>> PlayListItems { get; }
        public Dictionary<string, List<VideoInfo>> SearchList { get; private set; }

        public YouTubeListManagerCache()
        {
            PlayLists = new List<PlayList>();
            PlayListItems = new Dictionary<string, List<PlayListItem>>();
            SearchList = new Dictionary<string, List<VideoInfo>>();
        }

        public List<VideoInfo> GetSearchList(string title)
        {
            if (!SearchList.ContainsKey(title))
            {
                SearchList = new Dictionary<string, List<VideoInfo>>();
                SearchList[title] = new List<VideoInfo>();
            }

            return SearchList[title];
        }

        public List<PlayListItem> GetPlayListItems(string playListId)
        {
            if (!PlayListItems.ContainsKey(playListId))
                PlayListItems[playListId] = new List<PlayListItem>();

            return PlayListItems[playListId];
        } 
    }
}