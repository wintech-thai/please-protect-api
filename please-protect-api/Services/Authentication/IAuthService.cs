using System.IdentityModel.Tokens.Jwt;
using Its.PleaseProtect.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Its.PleaseProtect.Api.Services
{
    public interface IAuthService
    {
        public UserToken Login(UserLogin userLogin);
        public UserToken RefreshToken(string token);
        public SecurityToken ValidateAccessToken(string accessToken, JwtSecurityTokenHandler tokenHandler);
        public Task<IdpResult> AddUserToIDP(MOrganizeRegistration orgUser);
        public Task<IdpResult> ChangeUserPasswordIdp(MUpdatePassword password);
        public Task<IdpResult> ChangeForgotUserPasswordIdp(MUpdatePassword password);
        public Task<IdpResult> UserLogoutIdp(string userName);
        public Task<IdpResult> UpdateUserIdp(MUser user);
    }
}
