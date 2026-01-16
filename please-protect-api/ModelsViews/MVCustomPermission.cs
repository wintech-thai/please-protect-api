using System.Diagnostics.CodeAnalysis;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVCustomPermission
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public List<ControllerNode> Permissions { get; set; } //structure เหมือนกับใน MCustomRole

        public MVCustomPermission()
        {
            Permissions = [];
        }
    }
}
