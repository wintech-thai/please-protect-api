using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("CustomRoles")]
    [Index(nameof(RoleName), IsUnique = false)]

    public class MCustomRole
    {
        [Key]
        [Column("role_id")]
        public Guid? RoleId { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }


        [Column("role_name")]
        public string? RoleName { get; set; }

        [Column("role_description")]
        public string? RoleDescription { get; set; }

        [Column("role_definition")]
        public string? RoleDefinition { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("level")]
        public string? Level { get; set; } /* UserRole, AdminRole, CustomerRole */


        [Column("role_created_date")]
        public DateTime? RoleCreatedDate { get; set; }


        [NotMapped]
        public List<ControllerNode> Permissions { get; set; }

        public MCustomRole()
        {
            RoleId = Guid.NewGuid();
            RoleCreatedDate = DateTime.UtcNow;

            Permissions = [];
        }
    }
}
