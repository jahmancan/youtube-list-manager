using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace YouTubeListManager.Data.Domain.Mapping
{
    public class VideoInfoMap : EntityTypeConfiguration<VideoInfo>
    {
        public VideoInfoMap()
        {
            ToTable("VideoInfo");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Hash);
            Property(t => t.Title);
            Property(t => t.Duration);
            Property(t => t.Live);
            Property(t => t.ThumbnailUrl);

            HasMany(t => t.PlayListItems).WithRequired(t => t.VideoInfo);
        }
    }
}