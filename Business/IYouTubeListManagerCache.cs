using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business
{
    public interface IYouTubeListManagerCache
    {
        List<PlayList> GetPlayLists();
        List<T> Get<T>(string cacheKey) where T : class;
        void AddPlayLists(IEnumerable<PlayList> playLists);
        void Add<T>(string title, IEnumerable<T> items) where T : class;
    }
}