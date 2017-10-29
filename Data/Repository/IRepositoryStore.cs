using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Repository
{
    public interface IRepositoryStore
    {
        IRepository<PlaylistItem> PlaylistItemRepository { get; }
        IRepository<Playlist> PlaylistRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<VideoInfo> VideoInfoRepository { get; } 

        void SaveChanges();
    }
}