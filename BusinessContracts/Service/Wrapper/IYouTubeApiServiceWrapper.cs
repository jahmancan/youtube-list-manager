using Google.Apis.YouTube.v3.Data;

namespace YouTubeListManager.BusinessContracts.Service.Wrapper
{
    public interface IYouTubeApiServiceWrapper
    {
        PlaylistItem GetPlayListItem(string hash);
        Video GetVideo(string hash);
        Playlist GetYouTubePlayList(string hash);
    }
}