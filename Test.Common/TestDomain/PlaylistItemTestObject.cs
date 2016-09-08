using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Test.Common.TestDomain
{
    public class PlaylistItemTestObject : PlayListItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}