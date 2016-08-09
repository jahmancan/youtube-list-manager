using System.Collections.Generic;
using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.Common.GoogleApiExtensions;

namespace YouTubeListManager.Common
{
    public partial class PlaylistItemListResponse : IYouTubeApiResponse<PlaylistItem>
    {
        public string ETag { get; set; }
        public IList<PlaylistItem> Items { get; set; }
    }
}
