using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Repository
{
    public interface IRepositoryStore
    {
        IRepository<Track> TrackRepository { get; }
        IPlayListRepository PlayListRepository { get; }
        IRepository<User> UserRepository { get; }

        void SaveChanges();
    }
}