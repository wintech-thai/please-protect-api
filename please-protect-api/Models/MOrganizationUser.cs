using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("OrganizationsUsers")]
    [Index(nameof(OrgCustomId), IsUnique = false)]
    [Index(nameof(OrgCustomId), nameof(UserId), IsUnique = true, Name = "OrgUser_Unique1")]
    public class MOrganizationUser
    {
        [Key]
        [Column("org_user_id")]
        public Guid? OrgUserId { get; set; }

        [Column("org_custom_id")]
        public string? OrgCustomId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; } /* ถ้าเป็น NULL คือ Pending user */

        [Column("user_name")]
        public string? UserName { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("roles_list")]
        public string? RolesList { get; set; }

        [Column("is_org_initial_user")]
        public string? IsOrgInitialUser { get; set; } /* YES or NO */

        [Column("user_status")]
        public string? UserStatus { get; set; } /* Pending, Active, Disabled */

        [Column("tmp_user_email")]
        public string? TmpUserEmail { get; set; } /* เก็บ email ชั่วคราวที่ได้ invite ไปหา user */

        [Column("previous_user_status")]
        public string? PreviousUserStatus { get; set; } /* เก็บ UserStatus ก่อนที่จะถูก Disabled ถ้า Enable ก็จะกลับมาใช้ PreviousUserStatus */

        [Column("invited_date")]
        public DateTime? InvitedDate { get; set; }

        [Column("invited_by")]
        public string? InvitedBy { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("custom_role_id")]
        public string? CustomRoleId { get; set; } /* ไปยัง MCustomRole */


        [NotMapped]
        public string? UserEmail { get; set; }
        public string? OrgName { get; set; }
        public string? OrgDesc { get; set; }

        [NotMapped]
        public List<string> Roles { get; set; }

        [NotMapped]
        public string? CustomRoleName { get; set; }

        [NotMapped]
        public string? CustomRoleDesc { get; set; }

        public MOrganizationUser()
        {
            OrgUserId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
            RolesList = "";
            Roles = [];
        }
    }
}
