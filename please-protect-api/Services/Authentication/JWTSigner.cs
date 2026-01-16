using Microsoft.IdentityModel.Tokens;

namespace Its.PleaseProtect.Api.Services
{
    public class JwtSigner : IJwtSigner
    {
        public JwtSigner()
        {
        }

        public static void ResetSigedKeyJson()
        {
            //For unit testing
            JwtSignerKey.ResetSigedKeyJson();
        }
 
        public SecurityKey GetSignedKey(string? url)
        {
            return JwtSignerKey.GetSignedKey(url);
        }
    }
}
