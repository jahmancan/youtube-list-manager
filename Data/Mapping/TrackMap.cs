using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Data.Mapping
{
    public class TrackMap : EntityTypeConfiguration<Track>
    {
        public TrackMap()
        {
            ToTable("YouTubeTrack");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Author).HasMaxLength(500);
            Property(t => t.Title).HasMaxLength(500);
            Property(t => t.Duration);
            Property(t => t.Live);

            HasMany(t => t.PlayLists).WithMany(t => t.Tracks);
        }
    }
}