using Serilog;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Authentications
{
    public abstract class AuthenticationHandlerProxyBase : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        protected abstract AuthenResult? AuthenticateBasic(PathComponent pc, byte[]? jwtBytes, HttpRequest request);
        protected abstract AuthenResult? AuthenticateBearer(PathComponent pc, byte[]? jwtBytes, HttpRequest request);

        [Obsolete]
        protected AuthenticationHandlerProxyBase(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var path = Request.Path.ToString();
            if (Request.Path.StartsWithSegments("/health"))
            {
                var id = new ClaimsIdentity();
                var pp = new ClaimsPrincipal(id);
                var tck = new AuthenticationTicket(pp, Scheme.Name);

                return AuthenticateResult.Success(tck);
            }

            if (!Request.Headers.TryGetValue("Authorization", out var authData))
            {
                var msg = "No Authorization header found";

                //ไม่ response body ที่ตรงนี้ เพราะจะทำให้ API ที่ไม่ต้อง authen ได้รับ string ที่ไม่ใช่ JSON กลับไปด้วย
                //await Response.WriteAsync(msg);

                return AuthenticateResult.Fail(msg);
            }

            var authHeader = AuthenticationHeaderValue.Parse(authData!);
            if (!authHeader.Scheme.Equals("Bearer") && !authHeader.Scheme.Equals("Basic"))
            {
                var msg = $"Unknown scheme [{authHeader.Scheme}]";
                await Response.WriteAsync(msg);

                return AuthenticateResult.Fail(msg);
            }

            var authResult = new AuthenResult();
            try
            {
                var pc = ServiceUtils.GetPathComponent(Request);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
                if (authHeader.Scheme.Equals("Basic"))
                {
                    authResult = await Task.Run(() => AuthenticateBasic(pc, credentialBytes, Request));
                }
                else
                {
                    //Bearer
                    authResult = await Task.Run(() => AuthenticateBearer(pc, credentialBytes, Request));
                }
            }
            catch (Exception e)
            {
                var msg = e.Message;
                await Response.WriteAsync(msg);

                Log.Error($"[AuthenticationHandlerProxyBase] --> [{msg}]");
                return AuthenticateResult.Fail($"Invalid Authorization Header for [{authHeader.Scheme}]");
            }

            if (authResult!.UserAuthen == null)
            {
                var msg = $"User not found [{authResult.UserName}], scheme=[{authHeader.Scheme}]";

                return AuthenticateResult.Fail(msg);
            }
            else if (authResult!.UserAuthen.Status != "OK")
            {
                var msg = $"User status invalid [{authResult.UserName}] status=[{authResult.UserAuthen.Status}]";
                await Response.WriteAsync(msg);

                return AuthenticateResult.Fail(msg);
            } 

            var identity = new ClaimsIdentity(authResult.UserAuthen.Claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Context.Request.Headers.Append("AuthenScheme", Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}