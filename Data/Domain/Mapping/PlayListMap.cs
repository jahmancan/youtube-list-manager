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
            Property(t => t.Hash);
            Property(t => t.Title).HasMaxLength(255);
            Property(t => t.UserId);
            Property(t => t.ThumbnailUrl);
            Property(t => t.PrivacyStatus);

            Ignore(t => t.ItemCount);

            HasOptional(t => t.User).WithMany(t => t.PlayLists).HasForeignKey(t => t.UserId);
            HasMany(t => t.PlayListItems).WithRequired(t => t.PlayList);
        }
    }
}