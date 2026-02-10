using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("NotiAlertEvents")]
    [Index(nameof(Name), IsUnique = false)]
    [Index(nameof(Summary), IsUnique = false)]
    [Index(nameof(Detail), IsUnique = false)]
    [Index(nameof(Severity), IsUnique = false)]

    public class MNotiAlertEvent
    {
        [Key]
        [Column("noti_alert_id")]
        public Guid? Id { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("summary")]
        public string? Summary { get; set; }

        [Column("status")]
        public string? Status { get; set; } /* Firing, Resolved */

        [Column("detail")]
        public string? Detail { get; set; }

        [Column("severity")]
        public string? Severity { get; set; }

        [Column("raw_data")]
        public string? RawData { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MNotiAlertEvent()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
