using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Configurations")]
    [Index(nameof(ConfigType), IsUnique = false)]
    [Index(nameof(ConfigValue), IsUnique = false)]

    public class MConfiguration
    {
        [Key]
        [Column("config_id")]
        public Guid? ConfigId { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("config_type")]
        public string? ConfigType { get; set; } /* Domain, OrgShortName, LogoUrl */

        [Column("config_value")]
        public string? ConfigValue { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MConfiguration()
        {
            ConfigId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
