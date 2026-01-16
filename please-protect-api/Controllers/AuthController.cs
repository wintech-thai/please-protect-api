using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService svc;
        private readonly IJobService _jobSvc;
        private readonly IUserService _userSvc;
        private readonly IRedisHelper _redis;

        [ExcludeFromCodeCoverage]
        public AuthController(
            IAuthService service,
            IRedisHelper redis,
            IJobService jobSvc,
            IUserService userSvc
            )
        {
            svc = service;
            _redis = redis;
            _jobSvc = jobSvc;
            _userSvc = userSvc;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/temp/action/Login")]
        public IActionResult Login([FromBody] UserLogin request)
        {
            var result = svc.Login(request);
            Response.HttpContext.Items.Add("Temp-Identity-Name", request.UserName);
            
            if (result.Status != "Success")
            {
                return Unauthorized("Unauthorized, incorrect user or password!!!");
            }

            var sessionKey = CacheHelper.CreateLoginSessionKey(request.UserName);
            var obj = new UserToken() { UserName = request.UserName };
            _ = _redis.SetObjectAsync(sessionKey, obj);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/temp/action/Refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = svc.RefreshToken(request.RefreshToken);
            Response.HttpContext.Items.Add("Temp-Identity-Name", result.UserName);

            var sessionKey = CacheHelper.CreateLoginSessionKey(result.UserName);

            if (result.Status != "Success")
            {
                _ = _redis.DeleteAsync(sessionKey);
                return Unauthorized("Unauthorized, incorrect refresh token!!!");
            }

            var obj = new UserToken() { UserName = result.UserName };
            _ = _redis.SetObjectAsync(sessionKey, obj);

            return Ok(result);
        }
    }
}
