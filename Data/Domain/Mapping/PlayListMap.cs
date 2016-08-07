using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace YouTubeListManager.Data.Domain.Mapping
{
    public class PlayListMap : EntityTypeConfiguration<PlayList>
    {
        public PlayListMap()
        {
            ToTable("PlayList");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Title).HasMaxLength(255);
            Property(t => t.UserId);
            Property(t => t.PrivacyStatus);

            HasOptional(t => t.User).WithMany(t => t.PlayLists).HasForeignKey(t => t.UserId);
            HasMany(t => t.Tracks).WithMany(t => t.PlayLists)
                .Map(m =>
                {
                    m.ToTable("PlayListTrack");
                    m.MapLeftKey("PlayListId");
                    m.MapRightKey("TrackId");
                });
        }
    }
}