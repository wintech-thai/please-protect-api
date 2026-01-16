using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Its.PleaseProtect.Api.ModelsViews;
using Microsoft.AspNetCore.Identity;
using RulesEngine.Models;

namespace Its.PleaseProtect.Api.Utils
{
    public static class ServiceUtils
    {
        private static readonly Random _random = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly string[] whiteListedApi = [
                "OnlyUser:GetUserAllowedOrg",
                "OnlyUser:UpdatePassword",
                "OnlyUser:GetUserByUserName",
                "OnlyUser:UpdateUserByUserName",
                "OnlyUser:Logout"
            ];

        public static bool IsWhiteListedAPI(string controller, string api)
        {
            //จะไม่ต้อง verify user แต่ยังต้อง validate JWT token อยู่
            var whiteListedKey = $"{controller}:{api}";

            return whiteListedApi.Contains(whiteListedKey);
        }

        public static bool IsGuidValid(string guid)
        {
            try
            {
                Guid.Parse(guid);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int ReadInt32BigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
        }

        public static PathComponent GetPathComponent(HttpRequest request)
        {
            var path = request.Path;

            var pattern1 = @"^\/api\/(.+)\/org\/(.+)\/action\/(.+)$";
            MatchCollection matchesUserApi = Regex.Matches(path, pattern1, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            var pattern2 = @"^\/admin-api\/(.+)\/org\/(.+)\/action\/(.+)$";
            MatchCollection matchesAdminApi = Regex.Matches(path, pattern2, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            var pattern3 = @"^\/customer-api\/(.+)\/org\/(.+)\/action\/(.+)$";
            MatchCollection matchesCustomerApi = Regex.Matches(path, pattern3, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            var result = new PathComponent();
            if (matchesUserApi.Count > 0)
            {
                result.OrgId = matchesUserApi[0].Groups[2].Value;
                result.ControllerName = matchesUserApi[0].Groups[1].Value;
                result.ApiName = matchesUserApi[0].Groups[3].Value;
                result.ApiGroup = "user";
            }
            else if (matchesAdminApi.Count > 0)
            {
                result.OrgId = matchesAdminApi[0].Groups[2].Value;
                result.ControllerName = matchesAdminApi[0].Groups[1].Value;
                result.ApiName = matchesAdminApi[0].Groups[3].Value;
                result.ApiGroup = "admin";
            }
            else if (matchesCustomerApi.Count > 0)
            {
                result.OrgId = matchesCustomerApi[0].Groups[2].Value;
                result.ControllerName = matchesCustomerApi[0].Groups[1].Value;
                result.ApiName = matchesCustomerApi[0].Groups[3].Value;
                result.ApiGroup = "customer";
            }

            return result;
        }
    }
}
