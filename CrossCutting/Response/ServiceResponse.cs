namespace YouTubeListManager.CrossCutting.Response
{
    public class ServiceResponse<T>
    {
        public string NextPageToken { get; private set; }
        public T Response { get; private set; }
        
        public ServiceResponse(string nextPageToken, T response)
        {
            NextPageToken = nextPageToken;
            Response = response;
        }
    }
}