using System.Collections.Generic;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeUpdateService
    {
        void UpdatePlayLists(IEnumerable<PlayList> playLists);
    }
}