using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeListAPI.Business.Service.Response
{
    public interface IPlaylistResponseService
    {
        Task<PlaylistListResponse> GetResponse(string requestToken, string playListId);
    }
}