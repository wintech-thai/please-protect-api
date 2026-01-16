using Serilog;
using System.Security.Claims;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Utils;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.Authentications
{
    public class BearerAuthenticationRepo : IBearerAuthenticationRepo
    {
        private readonly IOrganizationService? service = null;
        private readonly IRedisHelper _redis;

        public BearerAuthenticationRepo(IOrganizationService svc, IRedisHelper redis)
        {
            service = svc;
            _redis = redis;
        }

        private MVOrganizationUser? VerifyUser(string orgId, string user, HttpRequest request)
        {
            //จะมี API บางตัวที่ไม่ต้องสนใจ user ว่าอยู่ใน org มั้ยเช่น UpdatePassword, GetAllAllowedOrg...

            var pc = ServiceUtils.GetPathComponent(request);
            var isWhiteListed = ServiceUtils.IsWhiteListedAPI(pc.ControllerName, pc.ApiName);

            if (isWhiteListed)
            {
                var ou = new MVOrganizationUser()
                {
                    Status = "OK",
                    Description = $"Whitelisted API [{pc.ControllerName}] [{pc.ApiName}]",

                    User = new Models.MUser() { UserName = user },
                    OrgUser = new Models.MOrganizationUser() { OrgCustomId = orgId },
                };
                //Console.WriteLine($"WHITELISTED ======= [{pc.ApiName}] [{pc.ControllerName}] ====");
                return ou;
            }

            var key = $"#{orgId}:VerifyUser:#{user}";
            var t = _redis.GetObjectAsync<MVOrganizationUser>(key);
            var orgUser = t.Result;

            if (orgUser == null)
            {
                var m = service!.VerifyUserInOrganization(orgId, user);
                _ = _redis.SetObjectAsync(key, m, TimeSpan.FromMinutes(5));

                orgUser = m;
            }

            if (orgUser != null)
            {
                // Check ตรงนี้หลังจากที่มี verify แล้วมี user อยู่ใน Organization
                // Check ต่อว่ามี Session อยู่ใน Redis ที่ setup ไว้ตอนที่ login หรือไม่
                // ต้องเช็คตรงนี้เพื่อทำเรื่องการ logout (เรียก API /logout) แบบทันที session ต้องหลุด

                var sessionKey = CacheHelper.CreateLoginSessionKey(user);
                var session = _redis.GetObjectAsync<UserToken>(sessionKey);
                if (session.Result == null)
                {
                    var ou = new MVOrganizationUser()
                    {
                        Status = "USER_SESSION_NOT_FOUND",
                        Description = $"Session not found please re-login for username [{user}]",

                        User = new MUser() { UserName = user },
                        OrgUser = new MOrganizationUser() { OrgCustomId = orgId },
                    };

                    return ou;
                }
            }

            return orgUser;
        }

        public User? Authenticate(string orgId, string user, string password, HttpRequest request)
        {
            var m = VerifyUser(orgId, user, request);
            if (m == null)
            {
                return null;
            }

            if (!m.Status!.Equals("OK"))
            {
                Log.Information(m.Description!);
                return null;
            }

            var u = new User()
            {
                UserName = user,
                Password = "",
                UserId = m.User!.UserId,
                Role = m.OrgUser!.RolesList,
                AuthenType = "JWT",
                OrgId = m.OrgUser.OrgCustomId,
                Email = m.User.UserEmail,
                CustomRoleId = m.OrgUser.CustomRoleId,
                CustomRoleName = m.OrgUser.CustomRoleName,

                Status = m.Status,
                Description = m.Description,
            };

            u.Claims = [
                new Claim(ClaimTypes.NameIdentifier, u.UserId.ToString()!),
                new Claim(ClaimTypes.Name, user),
                new Claim(ClaimTypes.Role, u.Role!),
                new Claim(ClaimTypes.PrimaryGroupSid, $"{u.CustomRoleId}:{u.CustomRoleName}"),
                new Claim(ClaimTypes.AuthenticationMethod, u.AuthenType!),
                new Claim(ClaimTypes.Uri, request.Path),
                new Claim(ClaimTypes.GroupSid, u.OrgId!),
            ];

            return u;
        }
    }
}
