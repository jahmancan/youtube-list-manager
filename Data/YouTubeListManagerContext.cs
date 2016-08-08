using System.Data.Entity;
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

        public DbSet<PlayListItem> PlayListItems { get; set; }
        public DbSet<PlayList> PlayLists { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VideoInfo> Video { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PlayListItemMap());
            modelBuilder.Configurations.Add(new PlayListMap());
            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new VideoInfoMap());
        }
    }
}