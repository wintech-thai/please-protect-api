using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Controllers
{
    public class IdentityValidationResult
    {
        public string? UserName { get; set; }
        public string? CustomerId { get; set; }
        public ObjectResult? RequestResult { get; set; }
    }

    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class OnlyUserController : ControllerBase
    {
        private readonly IUserService svc;
        private readonly IOrganizationService _orgSvc;
        private readonly IRedisHelper _redis;

        public OnlyUserController(
            IUserService service,
            IRedisHelper redis,
            IOrganizationService orgService)
        {
            svc = service;
            _orgSvc = orgService;
            _redis = redis;
        }

        private IdentityValidationResult ValidateUserIdentity()
        {
            var result = new IdentityValidationResult();

            var idTypeObj = Response.HttpContext.Items["Temp-Identity-Type"];
            if (idTypeObj == null)
            {
                var obj = BadRequest("Unable to identify identity type!!!");
                result.RequestResult = obj;

                return result;
            }

            var idType = idTypeObj.ToString();
            if (idType != "JWT")
            {
                var obj = BadRequest("Only allow for JWT identity type!!!");
                result.RequestResult = obj;

                return result;
            }

            var nameObj = Response.HttpContext.Items["Temp-Identity-Name"];
            if (nameObj == null)
            {
                var obj = BadRequest("Unable to find user name!!!");
                result.RequestResult = obj;

                return result;
            }

            var userName = nameObj.ToString();
            if (userName == "")
            {
                var obj = BadRequest("User name is empty!!!");
                result.RequestResult = obj;

                return result;
            }

            result.UserName = userName;

            return result;
        }

        [HttpGet]
        [Route("org/{id}/action/GetUserAllowedOrg")]
        public IActionResult GetUserAllowedOrg()
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var userName = validateResult.UserName;

            //ใช้ userName ที่มาจาก JWT เท่านั้น
            var result = _orgSvc.GetUserAllowedOrganization(userName!);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdatePassword")]
        public IActionResult UpdatePassword(string id, [FromBody] MUpdatePassword request)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var userName = validateResult.UserName;

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยน password เฉพาะของตัวเองเท่านั้น
            var result = svc.UpdatePassword(userName, request);
            Response.Headers.Append("CUST_STATUS", result.Status);

            var message = $"{result.Description}";
            if (!string.IsNullOrEmpty(request.UserName) && (userName != request.UserName))
            {
                //เอาไว้ดูว่ามีใครลองส่ง username เข้ามาเพื่อ hack ระบบหรือไม่
                message = $"{message}, JWT user [{userName}] but injected user is [{request.UserName}]";
            }
            //Comment ไว้ก่อนเพราะถ้า validation password ผิด มันจะมีอักขระพิเศษที่ใส่ใน header ไม่ได้ 
            //Response.Headers.Append("CUST_DESC", message);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateUserByUserName/{userName}")]
        public IActionResult UpdateUserByUserName(string id, string userName, [FromBody] MUser request)
        {
            request.UserName = userName;

            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var uname = validateResult.UserName;

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยนข้อมูลเฉพาะของตัวเองเท่านั้น
            var result = svc.UpdateUserByUserName(uname, request);
            Response.Headers.Append("CUST_STATUS", result.Status);

            var message = $"{result.Description}";
            if (!string.IsNullOrEmpty(request.UserName) && (uname != request.UserName))
            {
                //เอาไว้ดูว่ามีใครลองส่ง username เข้ามาเพื่อ hack ระบบหรือไม่
                message = $"{message}, JWT user [{uname}] but injected user is [{request.UserName}]";
            }

            Response.Headers.Append("CUST_DESC", message);
            return Ok(result);
        }

        [HttpGet]
        [Route("org/{id}/action/GetUserByUserName/{userName}")]
        public IActionResult GetUserByUserName(string id, string userName)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var uname = validateResult.UserName;

            //ใช้ userName ที่มาจาก JWT เท่านั้นเพื่อรับประกันว่าเปลี่ยนข้อมูลเฉพาะของตัวเองเท่านั้น
            var result = svc.GetUserByUserName(uname);
            Response.Headers.Append("CUST_STATUS", result.Status);

            var message = $"{result.Description}";
            if (!string.IsNullOrEmpty(uname) && (userName != uname))
            {
                //เอาไว้ดูว่ามีใครลองส่ง username เข้ามาเพื่อ hack ระบบหรือไม่
                message = $"{message}, JWT user [{uname}] but injected user is [{userName}]";
            }
            
            Response.Headers.Append("CUST_DESC", message);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/Logout")]
        public IActionResult Logout(string id)
        {
            var validateResult = ValidateUserIdentity();
            if (string.IsNullOrEmpty(validateResult.UserName))
            {
                return validateResult.RequestResult!;
            }

            var userName = validateResult.UserName;

            //ใช้ userName ที่มาจาก JWT เท่านั้น
            var result = svc.UserLogout(userName);
            Response.Headers.Append("CUST_STATUS", result.Status);
            Response.Headers.Append("CUST_DESC", result.Description);

            var sessionKey = CacheHelper.CreateLoginSessionKey(userName);
            _ = _redis.DeleteAsync(sessionKey);

            return Ok(result);
        }
    }
}
