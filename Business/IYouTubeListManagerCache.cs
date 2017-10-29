using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business
{
    public interface IYouTubeListManagerCache
    {
        List<Playlist> GetPlayLists(Func<Playlist, bool> predicate);
        List<T> Get<T>(string cacheKey) where T : class;
        void AddPlayLists(IEnumerable<Playlist> playLists);
        void Add<T>(string title, IEnumerable<T> items) where T : class;
    }
}