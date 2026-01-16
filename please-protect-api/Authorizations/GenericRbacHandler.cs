using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Utils;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.Authorizations;

public class GenericRbacHandler : AuthorizationHandler<GenericRbacRequirement>
{
    private readonly IRoleService service;
    private string apiCalled = "";
    private readonly IRedisHelper _redis;

    public GenericRbacHandler(IRoleService svc, IRedisHelper redis)
    {
        service = svc;
        _redis = redis;
    }

    private static Claim? GetClaim(string type, IEnumerable<Claim> claims)
    {
        var claim = claims.FirstOrDefault(x => x.Type == type);
        return claim;
    }

    private string GetApiGroup(string uri)
    {
        var userApiPattern = @"^\/api\/(.+)\/org\/(.+)\/action\/(.+)$";
        var userApimatches = Regex.Matches(uri, userApiPattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        if (userApimatches.Count > 0)
        {
            return "user";
        }

        return "";
    }

    private string? IsRoleUserValid(IEnumerable<MRole>? roles, string uri, string customRole, string orgId)
    {
        var uriPattern = @"^\/api\/(.+)\/org\/(.+)\/action\/(.+)$";
        var matches = Regex.Matches(uri, uriPattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

        var group = matches[0].Groups[1].Value;
        var api = matches[0].Groups[3].Value;

        var tokens = api.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var apiName = tokens[0]; //บางครั้ง มี /xxxx/yyy ตามหลังต่อท้ายชื่อ API
        api = char.ToUpper(apiName[0]) + apiName.Substring(1); //บางครั้งเรียกมาเป็นขึ้นต้นด้วยตัวเล็ก

        var keyword = $"{group}:{api}";
        apiCalled = keyword;

        if (ServiceUtils.IsWhiteListedAPI(group, api))
        {
            //No need to check for permission just only for this API
            return "TEMP";
        }

        //เช็ค custom role ก่อน
        var parts = customRole.Split(':');
        var customRoleId = parts[0];

        var cacheKeyPrefix = CacheHelper.CreateCustomRoleCacheLoaderKey(orgId);
        var cacheKey = $"{cacheKeyPrefix}:{customRoleId}:{group}:{api}";

        if (!string.IsNullOrEmpty(customRoleId))
        {
//Console.WriteLine($"DEBUG100 - Checking custom role [{cacheKey}]");
            var t = _redis.GetObjectAsync<bool?>(cacheKey);
            var isSelected = t.Result;
//Console.WriteLine($"DEBUG101 - Is selected [{cacheKey}], [{isSelected}]");

            if (isSelected == true)
            {
//Console.WriteLine($"DEBUG102 - Use custom role [{customRole}], [{isSelected}]");
                return customRole;
            }
        }

        // ถ้าไม่ผ่าน custom role ค่อยไปเช็คใน role ปกติ
        foreach (var role in roles!)
        {
            var patterns = role.RoleDefinition!.Split(',').ToList();
            foreach (var pattern in patterns!)
            {
                Match m = Regex.Match(keyword, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
                if (m.Success)
                {
                    return role.RoleName;
                }
            }
        }

        return "";
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GenericRbacRequirement requirement)
    {
        var idClaim = GetClaim(ClaimTypes.NameIdentifier, context.User.Claims);
        if (idClaim == null)
        {
            //The authentication failed earlier
            return Task.CompletedTask;
        }

        var roleClaim = GetClaim(ClaimTypes.Role, context.User.Claims);
        if (roleClaim == null)
        {
            //The authentication failed earlier
            return Task.CompletedTask;
        }

        var customRoleClaim = GetClaim(ClaimTypes.PrimaryGroupSid, context.User.Claims);
        if (customRoleClaim == null)
        {
            //The authentication failed earlier
            return Task.CompletedTask;
        }

        var uriClaim = GetClaim(ClaimTypes.Uri, context.User.Claims);
        if (uriClaim == null)
        {
            //The authentication failed earlier
            return Task.CompletedTask;
        }

        var authMethodClaim = GetClaim(ClaimTypes.AuthenticationMethod, context.User.Claims);
        if (authMethodClaim == null)
        {
            //The authentication failed earlier
            return Task.CompletedTask;
        }

        var orgIdClaim = GetClaim(ClaimTypes.GroupSid, context.User.Claims);
        if (orgIdClaim == null)
        {
            //The authentication failed earlier
            return Task.CompletedTask;
        }

        var userNameClaim = GetClaim(ClaimTypes.Name, context.User.Claims);
        if (userNameClaim == null)
        {
            //The authentication failed earlier
            return Task.CompletedTask;
        }

        var uid = idClaim.Value;
        var role = roleClaim.Value;
        var uri = uriClaim.Value;
        var method = authMethodClaim.Value;
        var authorizeOrgId = orgIdClaim.Value;
        var userName = userNameClaim.Value;
        var customRoleId = customRoleClaim.Value;

        var apiGroup = GetApiGroup(uri);

        //TODO : อนาคตต้องแยก GetRoleList แยกระหว่าง user กับ admin
        var roles = service.GetRolesList("", role);

        var roleMatch = "";

        if (apiGroup == "user")
        {
            roleMatch = IsRoleUserValid(roles, uri, customRoleId, authorizeOrgId);
        }

        if (!roleMatch!.Equals(""))
        {
            context.Succeed(requirement);
        }

        var mvcContext = context.Resource as DefaultHttpContext;
        mvcContext!.HttpContext.Items["Temp-Authorized-Role"] = roleMatch;
        mvcContext!.HttpContext.Items["Temp-Authorized-CustomRole"] = customRoleId;
        mvcContext!.HttpContext.Items["Temp-API-Called"] = apiCalled;
        mvcContext!.HttpContext.Items["Temp-Identity-Type"] = method;
        mvcContext!.HttpContext.Items["Temp-Identity-Id"] = uid;
        mvcContext!.HttpContext.Items["Temp-Identity-Name"] = userName;

        if (apiGroup == "customer")
        {
            var parts = userName.Split(':');

            var orgId = "";
            var entityId = "";
            if (parts.Length == 3 && parts[0] == "customer")
            {
                orgId = parts[1];
                entityId = parts[2];
            }

            mvcContext!.HttpContext.Items["Temp-Customer-Id"] = entityId;
        }

        return Task.CompletedTask;
    }
}
