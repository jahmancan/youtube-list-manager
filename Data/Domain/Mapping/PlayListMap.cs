using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.Data.Domain.Mapping
{
    public class PlaylistMap : EntityTypeConfiguration<Playlist>
    {
        public PlaylistMap()
        {
            ToTable("Playlist");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Hash);
            Property(t => t.Title).HasMaxLength(255);
            Property(t => t.UserId);
            Property(t => t.ThumbnailUrl);
            Property(t => t.PrivacyStatus);

            Ignore(t => t.ItemCount);
            Ignore(t => t.PlaylistItemsNextPageToken);

            HasOptional(t => t.User).WithMany(t => t.Playlists).HasForeignKey(t => t.UserId);
            HasMany(t => t.PlaylistItems).WithRequired(t => t.Playlist);
        }
    }
}