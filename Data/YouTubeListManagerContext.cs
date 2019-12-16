using System.Data.Entity;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Domain.Mapping;

namespace YouTubeListManager.Data
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

        public DbSet<PlaylistItem> PlaylistItems { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VideoInfo> Video { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PlaylistItemMap());
            modelBuilder.Configurations.Add(new PlaylistMap());
            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new VideoInfoMap());
        }
    }
}