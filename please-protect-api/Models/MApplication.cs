using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    //[Table("Applications")]
    //[Index(nameof(AppName), IsUnique = false)]

    public class MApplication
    {
        [Key]
        [Column("app_id")]
        public Guid? AppId { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }


        [Column("app_name")]
        public string? AppName { get; set; }

        [Column("repo_url")]
        public string? RepoUrl { get; set; }

        [Column("namespace")]
        public string? Namespace { get; set; }

        [Column("path")]
        public string? Path { get; set; }

        [Column("branch")]
        public string? Branch { get; set; }

        [Column("directory")]
        public string? Directory { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        public MApplication()
        {
        }
    }
}
