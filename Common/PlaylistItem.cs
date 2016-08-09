using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.Common.GoogleApiExtensions;

namespace YouTubeListManager.Common
{
    public partial class PlaylistItem : IYouTubeObject
    {
        public string ETag { get; set; }
    }
}
