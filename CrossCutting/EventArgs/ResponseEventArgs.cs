using System.Threading.Tasks;
using Google.Apis.Requests;

namespace YouTubeListManager.CrossCutting.EventArgs
{
    public class ResponseEventArgs<TResponse> : System.EventArgs where TResponse : IDirectResponseSchema
    {
        public Task<TResponse> Response { get; private set; }

        public ResponseEventArgs(Task<TResponse> response)
        {
            Response = response;
        }
    }
}