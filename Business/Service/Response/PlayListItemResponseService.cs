using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Service.Wrapper;

namespace YouTubeListAPI.Business.Service.Response
{
    public class PlaylistItemResponseService : ResponseService<PlaylistItemListResponse>, IPlaylistItemResponseService
    {
        public PlaylistItemResponseService(IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper)
            : base(youTubeApiListServiceWrapper)
        {
            youTubeApiListServiceWrapper.PlaylistItemsFetched += PlaylistItemsFetched;
        }

        public Task<PlaylistItemListResponse> GetResponse(string requestToken, string playListId)
        {
            try
            {
                youTubeApiListServiceWrapper.ExecuteAsyncRequestPlayListItems(requestToken, playListId);
                return response;
            }
            finally
            {
                youTubeApiListServiceWrapper.PlaylistItemsFetched -= PlaylistItemsFetched;
            }
        }

        private void PlaylistItemsFetched(object sender, ResponseEventArgs<PlaylistItemListResponse> eventArgs)
        {
            if (eventArgs.Response == null) return;

            response = eventArgs.Response;
        }
    }
}