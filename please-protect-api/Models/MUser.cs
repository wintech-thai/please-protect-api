using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Users")]
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(UserEmail), IsUnique = true)]
    public class MUser
    {
        [Key]
        [Column("user_id")]
        public Guid? UserId { get; set; }
    
        [Column("user_name")]
        public string? UserName { get; set; }

        [Column("user_email")]
        public string? UserEmail { get; set; }


        [Column("name")]
        public string? Name { get; set; }
        [Column("lastname")]
        public string? LastName { get; set; }
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }
        [Column("secondary_email")]
        public string? SecondaryEmail { get; set; }
        [Column("phone_number_verified")]
        public string? PhoneNumberVerified { get; set; } /* YES or NO */
        [Column("secondary_email_verified")]
        public string? SecondaryEmailVerified { get; set; } /* YES or NO */


        [Column("is_org_initial_user")]
        public string? IsOrgInitialUser { get; set; } /* YES or NO */

        [Column("user_created_date")]
        public DateTime? UserCreatedDate { get; set; }

        public MUser()
        {
            UserId = Guid.NewGuid();
            UserCreatedDate = DateTime.UtcNow;
        }
    }
}
