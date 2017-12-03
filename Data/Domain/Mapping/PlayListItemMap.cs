using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.Data.Domain.Mapping
{
    internal class PlayListItemMap : EntityTypeConfiguration<PlaylistItem>
    {
        public PlayListItemMap()
        {
            ToTable("PlayListItem");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Hash);
            Property(t => t.VideoInfoId);
            Property(t => t.PlaylistId);
            Property(t => t.Position);
        }
    }
}
