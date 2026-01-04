using System.Diagnostics.CodeAnalysis;

namespace Its.Otep.Api.Utils
{
    [ExcludeFromCodeCoverage]
    public class PathComponent
    {
        public string OrgId {get; set;}
        public string ControllerName {get; set;}
        public string ApiName { get; set; }
        public string ApiGroup {get; set;} /* OrgUser, AdminUser, CustomerUser */

        public PathComponent()
        {
            ApiName = "";
            ControllerName = "";
            OrgId = "";
            ApiGroup = "";
        }
    }
}
