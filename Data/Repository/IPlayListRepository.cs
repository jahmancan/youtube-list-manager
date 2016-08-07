using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Repository
{
    public interface IPlayListRepository : IRepository<PlayList>
    {
        void Update(PlayList playList);
    }
}