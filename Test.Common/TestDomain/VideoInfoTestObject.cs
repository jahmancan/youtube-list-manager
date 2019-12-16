using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Test.Common.TestDomain
{
    public class VideoInfoTestObject : VideoInfo, ITestThumbnailObject
    {
        public ThumbnailDetailsType ThumbnailDetailsType { get; set; }
        public string Description { get; set; }
    }
}