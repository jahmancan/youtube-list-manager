namespace YouTubeListManager.Data.Domain
{
    public class YouTubeTrack
    {
        public int Id { get; set; }
        public string YouTubeTrackId { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public bool Live { get; set; }

        public virtual YouTubeList YouTubeList { get; set; }    
    }
}
