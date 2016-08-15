using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Test.Common.TestDomain
{
    public class VideoInfoTestObject : VideoInfo
    {
        public ThumbnailDetailsType ThumbnailDetailsType { get; set; }
        public string Description { get; set; }
    }
}