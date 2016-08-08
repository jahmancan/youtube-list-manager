using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Repository
{
    public interface IRepositoryStore
    {
        IRepository<PlayListItem> PlayListItemRepository { get; }
        IPlayListRepository PlayListRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<VideoInfo> VideoInfoRepository { get; } 

        void SaveChanges();
    }
}