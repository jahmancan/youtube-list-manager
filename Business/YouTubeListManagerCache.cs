using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business
{
    public class YouTubeListManagerCache : IYouTubeListManagerCache
    {
        private const string PlayListCacheKey = "playlist";

        public List<Playlist> GetPlayLists(Func<Playlist, bool> predicate)
        {
            var playLists = Get<Playlist>(PlayListCacheKey);
            return predicate != null ? playLists.Where(predicate).ToList() : playLists;
        }

        public List<T> Get<T>(string cacheKey) where T : class
        {
            var cachedItems = HttpRuntime.Cache[cacheKey] as List<T>;
            if (cachedItems == null)
            {
                var items = new List<T>();
                HttpRuntime.Cache.Insert(cacheKey, items, null, DateTime.Now.AddMinutes(2), TimeSpan.Zero);
                return items;
            }
            return cachedItems;
        }

        public void AddPlayLists(IEnumerable<Playlist> playLists)
        {
            Add(PlayListCacheKey, playLists);
        }

        public void Add<T>(string title, IEnumerable<T> items) where T : class
        {
            Get<T>(title).AddRange(items);
        }
    }
}