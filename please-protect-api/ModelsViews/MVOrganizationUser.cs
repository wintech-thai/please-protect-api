using System.Diagnostics.CodeAnalysis;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVOrganizationUser
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MOrganizationUser? OrgUser { get; set; }
        public MUser? User { get; set; }
        public string? RegistrationUrl { get; set; }
    }
}
