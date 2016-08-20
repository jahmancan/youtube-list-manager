using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeListAPI.Business.Service.Response
{
    public interface ISearchListResponseService
    {
        Task<SearchListResponse> GetResponse(string requestToken, string title, SearchResource.ListRequest.VideoDurationEnum videoDuration);
    }
}