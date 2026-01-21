using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Iocs")]
    [Index(nameof(IocType), IsUnique = false)]
    [Index(nameof(IocValue), IsUnique = false)]
    [Index(nameof(DataSet), IsUnique = false)]
    [Index(nameof(IocSubType), IsUnique = false)]

    public class MIoC
    {
        [Key]
        [Column("ioc_id")]
        public Guid? IocId { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("dataset")]
        public string? DataSet { get; set; }

        [Column("ioc_type")]
        public string? IocType { get; set; } /* SrcIP, DstIP, Domain, Username, Email, FileHash256, FileHashMD5 */

        [Column("ioc_sub_type")]
        public string? IocSubType { get; set; }

        [Column("ioc_value")]
        public string? IocValue { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("last_seen_date")]
        public DateTime? LastSeenDate { get; set; }

        public MIoC()
        {
            IocId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
