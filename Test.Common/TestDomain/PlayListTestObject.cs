using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Test.Common.TestDomain
{
    public class PlayListTestObject : PlayList, ITestThumbnailObject
    {
        public ThumbnailDetailsType ThumbnailDetailsType { get; set; }
    }
}
