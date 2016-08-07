using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace YouTubeListManager.Data.Domain.Mapping
{
    public class TrackMap : EntityTypeConfiguration<Track>
    {
        public TrackMap()
        {
            ToTable("Track");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Title);
            Property(t => t.Duration);
            Property(t => t.Live);
            Property(t => t.ThumbnailUrl);
        }
    }
}