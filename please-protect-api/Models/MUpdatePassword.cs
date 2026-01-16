using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MUpdatePassword
    {
        public string UserName { get; set; }
        public string CurrentPassword { get; set; } /* Pass to Keycloak */
        public string NewPassword { get; set; } /* Pass to Keycloak */

        public MUpdatePassword()
        {
            UserName = "";
            CurrentPassword = "";
            NewPassword = "";
        }
    }
}
