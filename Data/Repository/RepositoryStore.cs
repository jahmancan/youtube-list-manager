using System;
using System.Data.Entity.Validation;
using Data;
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

        public IRepository<YouTubeTrack> YouTubeTrackRepository { get; private set; }
        public IRepository<YouTubeList> YouTubeListRepository { get; private set; }
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
            YouTubeTrackRepository = new Repository<YouTubeTrack>(context);
            YouTubeListRepository = new Repository<YouTubeList>(context);
            UserRepository = new Repository<User>(context);
        }
    }
}