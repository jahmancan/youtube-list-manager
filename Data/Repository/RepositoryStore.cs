using System;
using System.Data.Entity.Validation;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Extension;

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

        public IRepository<Track> TrackRepository { get; private set; }
        public IPlayListRepository PlayListRepository { get; private set; }
        public IRepository<User> UserRepository { get; private set; }

        public void SaveChanges()
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException entityValidationException)
            {
                entityValidationException.ShowDebugValidationException();
                throw new Exception("EntityValidationError", entityValidationException);
            }
        }

        private void Initialze()
        {
            TrackRepository = new Repository<Track>(context);
            PlayListRepository = new PlayListRepository(context);
            UserRepository = new Repository<User>(context);
        }
    }
}