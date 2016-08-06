using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Mapping
{
    public class PlayListMap : EntityTypeConfiguration<PlayList>
    {
        public PlayListMap()
        {
            ToTable("YouTubeList");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Title).HasMaxLength(500);

            HasRequired(t => t.User).WithMany(t => t.PlayLists);
            HasMany(t => t.Tracks).WithMany(t => t.PlayLists);
        }
    }
}