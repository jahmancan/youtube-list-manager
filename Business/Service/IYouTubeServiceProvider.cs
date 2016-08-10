using Google.Apis.YouTube.v3;

namespace YouTubeListAPI.Business.Service
{
    public interface IYouTubeServiceProvider
    {
        YouTubeService GetInstance();
    }
}