namespace YouTubeListManager.CrossCutting.Domain
{
    public class PlaylistItem
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public long? Position { get; set; }

        public int VideoInfoId { get; set; }
        public virtual VideoInfo VideoInfo { get; set; }
        public int PlaylistId { get; set; }
        public virtual Playlist Playlist { get; set; }
    }
}