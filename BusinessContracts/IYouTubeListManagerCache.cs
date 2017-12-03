using System;
using System.Collections.Generic;
using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.BusinessContracts
{
    public interface IYouTubeListManagerCache
    {
        List<Playlist> GetPlaylists(Func<Playlist, bool> predicate);
        List<T> Get<T>(string cacheKey) where T : class;
        void AddPlayLists(IEnumerable<Playlist> playLists);
        void Add<T>(string title, IEnumerable<T> items) where T : class;
    }
}