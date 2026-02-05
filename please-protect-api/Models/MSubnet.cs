using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Subnets")]
    [Index(nameof(Cidr), IsUnique = false)]
    [Index(nameof(Name), IsUnique = false)]

    public class MSubnet
    {
        [Key]
        [Column("subnet_id")]
        public Guid? Id { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("cidr")]
        public string? Cidr { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MSubnet()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
