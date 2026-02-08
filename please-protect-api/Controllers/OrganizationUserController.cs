using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.ViewsModels;
using System.Text.Json;
using System.Text;
using System.Web;
using Its.PleaseProtect.Api.Utils;

namespace Prom.LPR.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "GenericRolePolicy")]
    [Route("/api/[controller]")]
    public class OrganizationUserController : ControllerBase
    {
        private readonly IOrganizationUserService svc;
        private readonly IJobService _jobSvc;
        private readonly IRedisHelper _redis;

        [ExcludeFromCodeCoverage]
        public OrganizationUserController(IOrganizationUserService service, IJobService jobSvc, IRedisHelper redis)
        {
            svc = service;
            _jobSvc = jobSvc;
            _redis = redis;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddUser")]
        public MVOrganizationUser? AddUser(string id, [FromBody] MOrganizationUser request)
        {
            var result = svc.AddUser(id, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/InviteUser")]
        public MVOrganizationUser? InviteUser(string id, [FromBody] MOrganizationUser request)
        {
            var invitedByName = Response.HttpContext.Items["Temp-Identity-Name"];
            if (invitedByName == null)
            {
                invitedByName = "Unknown";
            }

            request.InvitedBy = invitedByName.ToString();

            var result = svc.InviteUser(id, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);
            Response.Headers.Append("CUST_DESC", result!.Description);

            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteUserById/{userId}")]
        public IActionResult DeleteUserById(string id, string userId)
        {
            var result = svc.DeleteUserById(id, userId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetUserById/{userId}")]
        public IActionResult GetUserById(string id, string userId)
        {
            var result = svc.GetUserByIdLeftJoin(id, userId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetUsers")]
        public IActionResult GetUsers(string id, [FromBody] VMOrganizationUser param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetUsersLeftJoin(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetUserCount")]
        public IActionResult GetUserCount(string id, [FromBody] VMOrganizationUser param)
        {
            var result = svc.GetUserCountLeftJoin(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateUserById/{userId}")]
        public IActionResult UpdateUserById(string id, string userId, [FromBody] MOrganizationUser request)
        {
            var result = svc.UpdateUserById(id, userId, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableUserById/{userId}")]
        public IActionResult EnableUserById(string id, string userId)
        {
            var result = svc.UpdateUserStatusById(id, userId, "Active");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableUserById/{userId}")]
        public IActionResult DisableUserById(string id, string userId)
        {
            var result = svc.UpdateUserStatusById(id, userId, "Disabled");

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        private (string, MVJob?) CreateEmailForgotPasswordJob(string orgId, MUserRegister reg)
        {
            var regType = "forgot-password";

            var jsonString = JsonSerializer.Serialize(reg);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            string jsonStringB64 = Convert.ToBase64String(jsonBytes);

            var dataUrlSafe = HttpUtility.UrlEncode(jsonStringB64);

            var registerDomain = "<REGISTER_SERVICE_DOMAIN>"; //คนที่เรียกใช้งานจะต้องเปลี่ยนเป็น domain ของ register service เอง

            var token = Guid.NewGuid().ToString();
            var registrationUrl = $"https://{registerDomain}/{regType}/{orgId}/{token}?data={dataUrlSafe}";

            var templateType = "user-forgot-password";
            var job = new MJob()
            {
                Name = $"{Guid.NewGuid()}",
                Description = "OrganizationUser.CreateEmailForgotPasswordJob()",
                Type = "SimpleEmailSend",
                Status = "Pending",
                Tags = templateType,

                Parameters =
                [
                    new MKeyValue { Name = "EMAIL_NOTI_ADDRESS", Value = "pjame.fb@gmail.com" },
                    new MKeyValue { Name = "EMAIL_OTP_ADDRESS", Value = reg.Email },
                    new MKeyValue { Name = "USER_NAME", Value = reg.UserName },
                    new MKeyValue { Name = "TEMPLATE_TYPE", Value = templateType },
                    new MKeyValue { Name = "USER_ORG_ID", Value = orgId },
                    new MKeyValue { Name = "RESET_PASSWORD_URL", Value = registrationUrl },
                ]
            };

            var result = _jobSvc.AddJob(orgId, job);

            //ใส่ data ไปที่ Redis เพื่อให้ register service มาดึงข้อมูลไปใช้ต่อ
            var cacheKey = CacheHelper.CreateApiOtpKey(orgId, "UserForgotPassword");
            _ = _redis.SetObjectAsync($"{cacheKey}:{token}", reg, TimeSpan.FromMinutes(60 * 24)); //หมดอายุ 1 วัน

            return (registrationUrl, result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetForgotPasswordLink/{orgUserId}")]
        public IActionResult GetForgotPasswordLink(string id, string orgUserId)
        {
            //ต้องใช้งานอย่างระมัดระวัง อย่างไป grant สิทธ์ให้ user แบบมั่ว ๆ ซั่ว ๆ นะ
            //จริง ๆ ควรต้องส่งไปยัง email เลยแต่ใน OTEP ไม่มีระบบ email
            var mv = new MVOrganizationUserRegistration()
            {
                Status = "OK",
                Description = "Success"
            };

            var svcStatus = svc.GetUserByIdLeftJoin(id, orgUserId);
            if (svcStatus.Status != "OK")
            {
                Response.Headers.Append("CUST_STATUS", svcStatus.Status);
                return Ok(svcStatus);
            }

            var user = svcStatus.OrgUser!;
            if (user == null)
            {
                mv.Status = "EMPTY_USER_RETURN";
                mv.Description = $"No user return for org user ID [{orgUserId}] !!!";

                Response.Headers.Append("CUST_STATUS", mv.Status);
                return Ok(mv);
            }

            if (user.UserStatus != "Active")
            {
                mv.Status = "USER_NOT_ACTIVE";
                mv.Description = $"User status is [{user.UserStatus}] for org user ID [{orgUserId}] !!!";

                Response.Headers.Append("CUST_STATUS", mv.Status);
                return Ok(mv);
            }

            var reg = new MUserRegister()
            {
                Email = user.UserEmail,
                UserName = user.UserName!,
                OrgUserId = user.UserId!.ToString(),
            };
            var (forgotPasswordUrl, result) = CreateEmailForgotPasswordJob(id, reg);
            mv.ForgotPasswordUrl = forgotPasswordUrl;

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(mv);
        }
    }
}
