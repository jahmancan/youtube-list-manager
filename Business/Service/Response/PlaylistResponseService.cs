using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.BusinessContracts.Service.Response;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.CrossCutting.EventArgs;

namespace YouTubeListAPI.Business.Service.Response
{
    public class PlaylistResponseService : ResponseService<PlaylistListResponse>, IPlaylistResponseService
    {
        public PlaylistResponseService(IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper) : base(youTubeApiListServiceWrapper)
        {
            youTubeApiListServiceWrapper.PlaylistFetched += PlayListFetched;
        }

        public async Task<PlaylistListResponse> GetResponse(string requestToken, string playListId)
        {
            try
            {
                if (string.IsNullOrEmpty(playListId))
                    return await youTubeApiListServiceWrapper.ExcuteAsyncRequestPlayLists(requestToken);
                else
                    return await youTubeApiListServiceWrapper.ExecuteAsyncRequestPlayList(requestToken, playListId);

                //return response;
            }
            finally
            {
                youTubeApiListServiceWrapper.PlaylistFetched -= PlayListFetched;
            }
        }

        private void PlayListFetched(object sender, ResponseEventArgs<PlaylistListResponse> eventArgs)
        {
            if (eventArgs.Response == null) return;

            response = eventArgs.Response;
        }
    }
}