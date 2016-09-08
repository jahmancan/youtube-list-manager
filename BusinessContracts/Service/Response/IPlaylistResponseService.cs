using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeListManager.BusinessContracts.Service.Response
{
    public interface IPlaylistResponseService
    {
        Task<PlaylistListResponse> GetResponse(string requestToken, string playListId);
    }
}