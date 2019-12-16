using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.CrossCutting.Request;

namespace YouTubeListManager.BusinessContracts.Service.Response
{
    public interface ISearchListResponseService
    {
        Task<SearchListResponse> GetResponseAsync(SearchRequest searchRequest);
    }
}