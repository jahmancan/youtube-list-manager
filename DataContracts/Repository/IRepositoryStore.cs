using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.DataContracts.Repository
{
    public interface IRepositoryStore
    {
        IRepository<PlaylistItem> PlayListItemRepository { get; }
        IRepository<Playlist> PlayListRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<VideoInfo> VideoInfoRepository { get; } 

        void SaveChanges();
    }
}