using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeListService
    {
        IEnumerable<Track> GetTracksBy(string playListId);
        IEnumerable<PlayList> GetPlaylists(string playListId = default(string), bool withTracks = false);

        IEnumerable<Track> ShowSuggestions(string title);

        void UpdateLists(IEnumerable<PlayList> playLists);
    }
}
