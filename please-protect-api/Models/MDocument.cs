using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Documents")]
    [Index(nameof(DocName), IsUnique = false)]

    public class MDocument
    {
        [Key]
        [Column("doc_id")]
        public Guid? DocId { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }


        [Column("doc_name")]
        public string? DocName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("metadata")]
        public string? MetaData { get; set; } /* JSON string */

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("doc_type")]
        public string? DocType { get; set; } /* แล้วแต่จะตั้ง เป็นตัวบอก job ที่แกะ metadata ว่าจะต้องทำอะไร */

        [Column("bucket")]
        public string? Bucket { get; set; }

        [Column("path")]
        public string? Path { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MDocument()
        {
            DocId = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
