using System.Threading.Tasks;
using Google.Apis.Requests;

namespace YouTubeListAPI.Business.Service.Response
{
    public class ResponseEventArgs<TResponse> where TResponse : IDirectResponseSchema
    {
        public Task<TResponse> Response { get; private set; }

        public ResponseEventArgs(Task<TResponse> response)
        {
            Response = response;
        }
    }
}