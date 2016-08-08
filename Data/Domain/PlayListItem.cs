namespace YouTubeListManager.Data.Domain
{
    public class PlayListItem
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public long? Position { get; set; }

        public int VideoInfoId { get; set; }
        public virtual VideoInfo VideoInfo { get; set; }
        public int PlayListId { get; set; }
        public virtual PlayList PlayList { get; set; }
    }
}