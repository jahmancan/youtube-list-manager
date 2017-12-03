using PlaylistItem = YouTubeListManager.CrossCutting.Domain.PlaylistItem;

namespace YouTubeListManager.Test.Common.TestDomain
{
    public class PlaylistItemTestObject : PlaylistItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}