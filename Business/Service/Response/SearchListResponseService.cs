using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Service.Wrapper;

namespace YouTubeListAPI.Business.Service.Response
{
    public class SearchListResponseService : ResponseService<SearchListResponse>, ISearchListResponseService
    {
        public SearchListResponseService(IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper)
            : base(youTubeApiListServiceWrapper)
        {
            youTubeApiListServiceWrapper.SearchResultsFetched += SearchResultsFetched;
        }

        public Task<SearchListResponse> GetResponse(string requestToken, string title, SearchResource.ListRequest.VideoDurationEnum videoDuration)
        {
            try
            {
                youTubeApiListServiceWrapper.ExecuteAsyncRequestSearch(requestToken, title, videoDuration);
                return response;
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