using System;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.Data.Extension;
using YouTubeListManager.DataContracts.Repository;

namespace YouTubeListManager.Data.Repository
{
    public class RepositoryStore : IRepositoryStore
    {
        private readonly YouTubeListManagerContext context;

        public RepositoryStore(YouTubeListManagerContext context)
        {
            this.context = context;

            Initialze();
        }

        public IRepository<PlaylistItem> PlaylistItemRepository { get; private set; }
        public IRepository<Playlist> PlaylistRepository { get; private set; }
        public IRepository<User> UserRepository { get; private set; }
        public IRepository<VideoInfo> VideoInfoRepository { get; private set; }

        public async Task SaveChangesAsync()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbEntityValidationException entityValidationException)
            {
                entityValidationException.ShowDebugValidationException();
                throw new Exception("EntityValidationError", entityValidationException);
            }
        }

        private void Initialze()
        {
            PlaylistItemRepository = new Repository<PlaylistItem>(context);
            PlaylistRepository = new Repository<Playlist>(context);
            UserRepository = new Repository<User>(context);
            VideoInfoRepository = new Repository<VideoInfo>(context);
        }
    }
}