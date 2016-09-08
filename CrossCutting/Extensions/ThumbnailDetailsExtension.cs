using Google.Apis.YouTube.v3.Data;

namespace YouTubeListManager.CrossCutting.Extensions
{
    public static class ThumbnailDetailsExtension
    {
        public static string GetThumbnailUrl(this ThumbnailDetails thumbnailDetails)
        {
            if (thumbnailDetails == null)
                return string.Empty;

            return (thumbnailDetails.Standard != null) ? thumbnailDetails.Standard.Url : thumbnailDetails.Default__.Url;
        }
    }
}