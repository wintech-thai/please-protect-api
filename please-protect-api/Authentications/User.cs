using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Its.PleaseProtect.Api.Authentications
{
    [ExcludeFromCodeCoverage]
    public class User
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        
        public Guid? UserId {get; set;}
        public string? UserName {get; set;}
        public string? Password {get; set;}
        public string? Role {get; set;}
        public string? CustomRoleId {get; set;}
        public string? CustomRoleName {get; set;}
        public string? AuthenType {get; set;}
        public string? OrgId {get; set;}
        public string? Email {get; set;}
        public IEnumerable<Claim>? Claims { get; set; }

        public User()
        {
            UserId = Guid.NewGuid();
        }
    }
}
