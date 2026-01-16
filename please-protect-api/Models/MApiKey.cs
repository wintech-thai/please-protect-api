using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("ApiKeys")]
    [Index(nameof(ApiKey), IsUnique = true)]
    [Index(nameof(KeyName), IsUnique = true)]
    [Index(nameof(OrgId))]
    public class MApiKey
    {
        [Key]
        [Column("key_id")]
        public Guid? KeyId { get; set; }
    
        [Column("api_key")]
        public string? ApiKey { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("key_name")]
        public string? KeyName { get; set; }

        [Column("key_created_date")]
        public DateTime? KeyCreatedDate { get; set; }

        [Column("key_expired_date")]
        public DateTime? KeyExpiredDate { get; set; }

        [Column("key_description")]
        public string? KeyDescription { get; set; }

        [Column("key_status")]
        public string? KeyStatus { get; set; } /* Active, Disabled */

        [Column("roles_list")]
        public string? RolesList { get; set; }

        [Column("custom_role_id")]
        public string? CustomRoleId { get; set; } /* ไปยัง MCustomRole */


        [NotMapped]
        public List<string> Roles { get; set; }

        [NotMapped]
        public string? CustomRoleName { get; set; }

        [NotMapped]
        public string? CustomRoleDesc { get; set; }

        public MApiKey()
        {
            KeyId = Guid.NewGuid();
            KeyCreatedDate = DateTime.UtcNow;
            Roles = [];
        }
    }
}
