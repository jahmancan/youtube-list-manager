using System.Data.Entity;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Mapping;

namespace Data
{
    public class YouTubeListManagerContext : DbContext
    {
        public YouTubeListManagerContext(string connectionString) 
            : base(connectionString)
        {
            Database.CreateIfNotExists();
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
            Database.Initialize(true);
        }

        public DbSet<YouTubeTrack> YouTubeTracks { get; set; }
        public DbSet<YouTubeList> YouTubeLists { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new YouTubeTrackMap());
            modelBuilder.Configurations.Add(new YouTubeListMap());
            modelBuilder.Configurations.Add(new UserMap());
        }
    }
}