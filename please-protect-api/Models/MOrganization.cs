using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Organizations")]
    [Index(nameof(OrgCustomId), IsUnique = true)]
    public class MOrganization
    {
        [Key]
        [Column("org_id")]
        public Guid? OrgId { get; set; }
    
        [Column("org_custom_id")]
        public string? OrgCustomId { get; set; }

        [Column("org_name")]
        public string? OrgName { get; set; }

        [Column("org_description")]
        public string? OrgDescription { get; set; }


        [Column("tags")]
        public string? Tags { get; set; } // Comma separated string

        [Column("addresses")]
        public string? Addresses { get; set; } // JSON string

        [Column("channels")]
        public string? Channels { get; set; } // JSON string

        [Column("logo_image_path")]
        public string? LogoImagePath { get; set; } // JSON string


        [NotMapped]
        public List<MKeyValue> AddressesArray { get; set; }
        [NotMapped]
        public List<MKeyValue> ChannelsArray { get; set; }
        [NotMapped]
        public string? LogoImageUrl { get; set; }

        [Column("org_created_date")]
        public DateTime? OrgCreatedDate { get; set; }

        public MOrganization()
        {
            OrgId = Guid.NewGuid();
            OrgCreatedDate = DateTime.UtcNow;

            AddressesArray = [];
            ChannelsArray = [];
        }
    }
}
