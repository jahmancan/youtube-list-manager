using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Repository
{
    public interface IRepositoryStore
    {
        IRepository<Track> TrackRepository { get; }
        IRepository<PlayList> PlayListRepository { get; }
        IRepository<User> UserRepository { get; }

        void SaveChanges();
    }
}