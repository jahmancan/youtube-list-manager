using Playlist = YouTubeListManager.CrossCutting.Domain.Playlist;

namespace YouTubeListManager.Test.Common.TestDomain
{
    public class PlayListTestObject : Playlist, ITestThumbnailObject
    {
        public ThumbnailDetailsType ThumbnailDetailsType { get; set; }
    }
}
