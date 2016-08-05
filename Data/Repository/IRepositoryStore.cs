using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Repository
{
    public interface IRepositoryStore
    {
        IRepository<YouTubeTrack> YouTubeTrackRepository { get; }
        IRepository<YouTubeList> YouTubeListRepository { get; }
        IRepository<User> UserRepository { get; }

        void SaveChanges();
    }
}