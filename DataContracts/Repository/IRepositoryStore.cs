using System.Threading.Tasks;
using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.DataContracts.Repository
{
    public interface IRepositoryStore
    {
        IRepository<PlaylistItem> PlaylistItemRepository { get; }
        IRepository<Playlist> PlaylistRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<VideoInfo> VideoInfoRepository { get; } 

        Task SaveChangesAsync();
    }
}