using System.Threading.Tasks;
using Google.Apis.Requests;
using YouTubeListAPI.Business.Service.Wrapper;

namespace YouTubeListAPI.Business.Service.Response
{
    public abstract class ResponseService<TResponse> where TResponse : IDirectResponseSchema
    {
        protected readonly IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper;
        protected Task<TResponse> response;

        protected ResponseService(IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper)
        {
            this.youTubeApiListServiceWrapper = youTubeApiListServiceWrapper;
        }
    }
}
