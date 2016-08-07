using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListService
    {
        IList<Track> GetTracksBy(string playListId, int maxResults = 50);
        IEnumerable<PlayList> GetPlaylists(int maxResults = 50);
    }
}
