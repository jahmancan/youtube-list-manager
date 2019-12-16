using System.Collections.Generic;
using Google.Apis.YouTube.v3;

namespace YouTubeListManager.CrossCutting.Request
{
    public class SearchRequest
    {
        public string NextPageRequestToken { get; set; }
        public string SearchKey { get; set; }
        public SearchResource.ListRequest.VideoDurationEnum VideoDuration { get; set; }
        public List<string> UsedVideoIdList { get; private set; }

        public SearchRequest()
        {
            UsedVideoIdList = new List<string>();
            VideoDuration = SearchResource.ListRequest.VideoDurationEnum.Any;
        }
    }
}
