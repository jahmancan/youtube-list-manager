using System;
using System.Collections.Generic;
using System.Web;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business
{
    public class YouTubeListManagerCache : IYouTubeListManagerCache
    {
        private const string playListCacheKey = "playlist";

        public List<PlayList> GetPlayLists()
        {
            return Get<PlayList>(playListCacheKey);
        }

        public List<T> Get<T>(string cacheKey) where T : class
        {
            var cachedItems = HttpRuntime.Cache[cacheKey] as List<T>;
            if (cachedItems == null)
            {
                var items = new List<T>();
                HttpRuntime.Cache.Insert(cacheKey, items, null, DateTime.Now.AddHours(1), TimeSpan.Zero);
                return items;
            }
            return cachedItems;
        }

        public void AddPlayLists(IEnumerable<PlayList> playLists)
        {
            Add(playListCacheKey, playLists);
        }

        public void Add<T>(string title, IEnumerable<T> items) where T : class
        {
            Get<T>(title).AddRange(items);
        }

        
    }
}