using Microsoft.IdentityModel.Tokens;

namespace Its.PleaseProtect.Api.Services
{
    public interface IJwtSigner
    {
        public SecurityKey GetSignedKey(string? url);
    }
}
