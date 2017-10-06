using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.BusinessContracts.Service.Response;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.CrossCutting.EventArgs;
using YouTubeListManager.CrossCutting.Request;

namespace YouTubeListAPI.Business.Service.Response
{
    public class SearchListResponseService : ResponseService<SearchListResponse>, ISearchListResponseService
    {
        public SearchListResponseService(IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper)
            : base(youTubeApiListServiceWrapper)
        {
            youTubeApiListServiceWrapper.SearchResultsFetched += SearchResultsFetched;
        }

        public async Task<SearchListResponse> GetResponseAsync(SearchRequest searchRequest)
        {
            try
            {
                youTubeApiListServiceWrapper.ExecuteAsyncRequestSearch(searchRequest);
                return await response;
            }
            finally
            {
                youTubeApiListServiceWrapper.SearchResultsFetched -= SearchResultsFetched;
            }
        }

        private void SearchResultsFetched(object sender, ResponseEventArgs<SearchListResponse> eventArgs)
        {
            if (eventArgs.Response == null) return;

            response = eventArgs.Response;
        }
    }
}