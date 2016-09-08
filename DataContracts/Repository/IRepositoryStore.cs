using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.DataContracts.Repository
{
    public interface IRepositoryStore
    {
        IRepository<PlayListItem> PlayListItemRepository { get; }
        IRepository<PlayList> PlayListRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<VideoInfo> VideoInfoRepository { get; } 

        void SaveChanges();
    }
}