using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.Data.Domain.Mapping
{
    internal class PlayListItemMap : EntityTypeConfiguration<PlayListItem>
    {
        public PlayListItemMap()
        {
            ToTable("PlayListItem");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Hash);
            Property(t => t.VideoInfoId);
            Property(t => t.PlayListId);
            Property(t => t.Position);
        }
    }
}
