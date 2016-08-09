using Google.Apis.YouTube.v3.Data;

namespace YouTubeListAPI.Business.Extensions
{
    internal static class ThumbnailDetailsExtension
    {
        internal static string GetThumbnailUrl(this ThumbnailDetails thumbnailDetails)
        {
            if (thumbnailDetails == null)
                return string.Empty;

            return (thumbnailDetails.Standard != null) ? thumbnailDetails.Standard.Url : thumbnailDetails.Default__.Url;
        }
    }
}