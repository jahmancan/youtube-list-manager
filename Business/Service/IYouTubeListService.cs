using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListService
    {
        IList<Track> GetTracksBy(string playListId);
        IEnumerable<PlayList> GetPlaylists(int maxResults = 100);
    }
}
