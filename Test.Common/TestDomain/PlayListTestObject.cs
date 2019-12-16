using Playlist = YouTubeListManager.CrossCutting.Domain.Playlist;

namespace YouTubeListManager.Test.Common.TestDomain
{
    public class PlaylistTestObject : Playlist, ITestThumbnailObject
    {
        public ThumbnailDetailsType ThumbnailDetailsType { get; set; }
    }
}
