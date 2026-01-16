using System.Text;
using System.Text.Encodings.Web;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Authentications
{
    public class AuthenticationHandlerProxy : AuthenticationHandlerProxyBase
    {
        private readonly IBasicAuthenticationRepo? basicAuthenRepo = null;
        private readonly IBearerAuthenticationRepo? bearerAuthRepo = null;
        private readonly IAuthService _authService;
        private JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        [Obsolete]
        public AuthenticationHandlerProxy(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IBasicAuthenticationRepo bsAuthRepo,
            IBearerAuthenticationRepo brAuthRepo,
            IAuthService authService,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            basicAuthenRepo = bsAuthRepo;
            bearerAuthRepo = brAuthRepo;
            _authService = authService;
        }

        protected override AuthenResult AuthenticateBasic(PathComponent pc, byte[]? jwtBytes, HttpRequest request)
        {
            var credentials = Encoding.UTF8.GetString(jwtBytes!).Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];

            var user = basicAuthenRepo!.Authenticate(pc.OrgId, username, password, request);
            var authResult = new AuthenResult()
            {
                UserAuthen = user,
                UserName = username,
            };

            return authResult;
        }

        protected override AuthenResult AuthenticateBearer(PathComponent pc, byte[]? jwtBytes, HttpRequest request)
        {
            var accessToken = Encoding.UTF8.GetString(jwtBytes!);

            //Throw exception if invalid
            _authService.ValidateAccessToken(accessToken, tokenHandler);

            var jwt = tokenHandler.ReadJwtToken(accessToken);
            string userName = jwt.Claims.First(c => c.Type == "preferred_username").Value;

            var user = new User();
            if (pc.ApiGroup == "user")
            {
                user = bearerAuthRepo!.Authenticate(pc.OrgId, userName, "", request);
            }

            var authResult = new AuthenResult()
            {
                UserAuthen = user,
                UserName = userName,
            };

            return authResult;
        }
    }
}