using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using YouTubeListManager.CrossCutting.Domain;

namespace YouTubeListManager.Data.Domain.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            ToTable("User");
            HasKey(t => t.Id);

            Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Name).IsRequired();
        }
    }
}