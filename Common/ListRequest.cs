using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeListManager.Common.GoogleApiExtensions;

namespace YouTubeListManager.Common
{
    public class ListRequest : YouTubeBaseServiceRequest<PlaylistItemListResponse>, IYouTubeRequest<PlaylistItemListResponse, PlaylistItem>
    {
        public ListRequest(IClientService service) : base(service)
        {
        }

        public override string MethodName { get; }
        public override string RestPath { get; }
        public override string HttpMethod { get; }
        public string Id { get; set; }
    }
}
