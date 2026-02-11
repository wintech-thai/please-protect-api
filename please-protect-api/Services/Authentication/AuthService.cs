
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Its.PleaseProtect.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Its.PleaseProtect.Api.Services
{
    public class KeycloakUser
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class AuthService : BaseService, IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string tokenEndpoint = "";
        private readonly string userEndpoint = "";
        private readonly string chagePasswordEndpoint = "";
        private readonly string updateUserEndpoint = "";
        private readonly string getUserIdEndpoint = "";
        private readonly string logoutEndpoint = "";
        private readonly string issuer = "";
        private readonly string signedKeyUrl = "";
        private readonly string? clientId = "";
        private readonly string? clientSecret = "";
        private readonly string deleteUserEndpoint = "";
        private IJwtSigner signer = new JwtSigner();


        public AuthService(IHttpClientFactory httpClientFactory) : base()
        {
            _httpClientFactory = httpClientFactory;
            var authPath = ""; //Keycloak เวอร์ชันใหม่ ๆ จะไม่มี /auth แล้ว

            var realm = Environment.GetEnvironmentVariable("IDP_REALM");
            var urlPrefix = Environment.GetEnvironmentVariable("IDP_URL_PREFIX");

            clientId = Environment.GetEnvironmentVariable("IDP_CLIENT_ID");
            clientSecret = Environment.GetEnvironmentVariable("IDP_CLIENT_SECRET");

            issuer = $"{urlPrefix}{authPath}/realms/{realm}";
//Console.WriteLine($"DEBUG_AUTH1 issuer=[{issuer}]");
            tokenEndpoint = $"{urlPrefix}{authPath}/realms/{realm}/protocol/openid-connect/token";
            signedKeyUrl = $"{urlPrefix}{authPath}/realms/{realm}/protocol/openid-connect/certs";

            userEndpoint = $"{urlPrefix}{authPath}/admin/realms/{realm}/users";
            chagePasswordEndpoint = $"{urlPrefix}{authPath}/admin/realms/{realm}/users/<<user-id>>/reset-password";
            updateUserEndpoint = $"{urlPrefix}{authPath}/admin/realms/{realm}/users/<<user-id>>";
            logoutEndpoint = $"{urlPrefix}{authPath}/admin/realms/{realm}/users/<<user-id>>/logout";
            getUserIdEndpoint = $"{urlPrefix}{authPath}/admin/realms/{realm}/users?username=<<user-name>>";
            deleteUserEndpoint = $"{urlPrefix}{authPath}/admin/realms/{realm}/users/<<user-id>>";
        }

        private string GetPreferredUsername(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            // อ่าน claim preferred_username
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            return username!;
        }

        private UserToken GetToken(KeyValuePair<string, string>[] form)
        {
            var userToken = new UserToken();
            userToken.Status = "Success";
//Console.WriteLine($"DEBUG_B1 [{tokenEndpoint}]");
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(form)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                var response = client.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    userToken.Status = response.StatusCode.ToString();
                    return userToken;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                userToken.Token = JsonSerializer.Deserialize<KeycloakTokenResponse>(content)!;
            }
            catch (HttpRequestException ex)
            {
                userToken.Status = "FAILED";
                userToken.Message = ex.Message;
            }

            return userToken;
        }

        private UserToken GetServiceAccountToken()
        {
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
            };
//Console.WriteLine($"DEBUG_A1 [{clientId}], [{clientSecret}]");
            var saToken = GetToken(form);
//Console.WriteLine($"DEBUG_A2 [{saToken.Status}]");
            return saToken;
        }

        private async Task<IdpResult> CreateUserAsync(string token, MOrganizeRegistration orgUser)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var user = new
            {
                username = orgUser.UserName,
                email = orgUser.Email,
                enabled = true,
                firstName = orgUser.Name,
                lastName = orgUser.Lastname,

                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = orgUser.UserInitialPassword,
                        temporary = false
                    }
                },
                groups = new string[] { },
                attributes = new
                {
                    locale = new[] { "en" }
                }
            };
//Console.WriteLine($"DEBUG_C1 [{userEndpoint}]");
            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(userEndpoint, content);

            var result = new IdpResult()
            {
                Success = true,
                Message = $"User [{orgUser.UserName}] [{orgUser.Email}] created successfully!",
            };

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                result.Message = await response.Content.ReadAsStringAsync();
//Console.WriteLine($"DEBUG_C2 [{result.Message}]");
            }

            return result;
        }

        private async Task<IdpResult> ChangeOwnPasswordAsync(MUpdatePassword password, string token, string userId)
        {
            var ep = chagePasswordEndpoint.Replace("<<user-id>>", userId);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                type = "password",
                value = password.NewPassword,
                temporary = false,
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(ep, jsonContent);

            var result = new IdpResult()
            {
                Success = true,
                Message = $"Successfully reset password for [{password.UserName}].",
            };

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                var errMsg = await response.Content.ReadAsStringAsync();
                result.Message = $"Uanble to call update password API, {errMsg}";
            }

            return result;
        }

        private async Task<IdpResult> UpdateUserAsync(MUser user, string token, string userId)
        {
            var ep = updateUserEndpoint.Replace("<<user-id>>", userId);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                firstName = user.Name,
                lastName = user.LastName,
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(ep, jsonContent);

            var result = new IdpResult()
            {
                Success = true,
                Message = $"Successfully update data for [{user.UserName}].",
            };

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                var errMsg = await response.Content.ReadAsStringAsync();
                result.Message = $"Uanble to call update user API, {errMsg}";
            }

            return result;
        }

        private async Task<IdpResult> DeleteUserAsync(string token, string userId)
        {
            var ep = deleteUserEndpoint.Replace("<<user-id>>", userId);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync(ep);

            var result = new IdpResult();

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                result.Success = true;
                result.Message = $"User [{userId}] deleted successfully.";
                return result;
            }

            // error case
            var errMsg = await response.Content.ReadAsStringAsync();

            result.Success = false;
            result.Message = $"Unable to delete user. Status: {(int)response.StatusCode}, {errMsg}";

            return result;
        }

        private async Task<IdpResult> LogoutUserAsync(string token, string userId)
        {
            var ep = logoutEndpoint.Replace("<<user-id>>", userId);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsync(ep, null);

            var result = new IdpResult()
            {
                Success = true,
                Message = $"Successfully logout user for IDP user=[{userId}].",
            };

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                var errMsg = await response.Content.ReadAsStringAsync();
                result.Message = $"Uanble to call LogoutUserAsync(), {errMsg}";
            }

            return result;
        }

        public async Task<IdpResult> GetUserIdByUsernameAsync(string username, string token)
        {
            var ep = getUserIdEndpoint.Replace("<<user-name>>", username);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(ep);

            var result = new IdpResult()
            {
                UserId = "",
                Success = true,
                Message = "",
            };

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                result.Success = false;
                result.Message = error;

                return result;
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<KeycloakUser>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var user = users?.FirstOrDefault();
            result.UserId = user?.Id;

            return result;
        }

        public SecurityToken ValidateAccessToken(string accessToken, JwtSecurityTokenHandler tokenHandler)
        {
            //Important : In Keycloak keys setting we must enable only 1 key proder 'RS256'.
            //https://keycloak.devops.napbiotec.io/auth/realms/rtarf-ads-dev/protocol/openid-connect/certs
            var securityKey = signer.GetSignedKey(signedKeyUrl);
            //Console.WriteLine($"=== {accessToken} ===");
            var param = new TokenValidationParameters()
            {
                ValidIssuer = issuer,
                ValidAudience = "account",
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };

            SecurityToken validatedToken;
            tokenHandler.ValidateToken(accessToken, param, out validatedToken);

            return validatedToken;
        }

        public UserToken Login(UserLogin userLogin)
        {
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "password"),
                new KeyValuePair<string,string>("response_type", "token"),
                new KeyValuePair<string,string>("scope", "openid offline_access"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
                new KeyValuePair<string,string>("username", userLogin.UserName),
                new KeyValuePair<string,string>("password", userLogin.Password)
            };
            //Console.WriteLine($"==== [{userLogin.Password}] ====");
            var userToken = GetToken(form);
            userToken.UserName = userLogin.UserName;

            return userToken;
        }

        public UserToken RefreshToken(string refreshToken)
        {
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "refresh_token"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
                new KeyValuePair<string,string>("refresh_token", refreshToken),
            };

            var userToken = GetToken(form);
            if (userToken.Status == "Success")
            {
                userToken.UserName = GetPreferredUsername(userToken.Token.AccessToken!);
            }

            return userToken;
        }

        public async Task<IdpResult> AddUserToIDP(MOrganizeRegistration orgUser)
        {
            var token = GetServiceAccountToken();
            return await CreateUserAsync(token.Token.AccessToken, orgUser);
        }

        public async Task<IdpResult> DeleteUserIdp(MUser user)
        {
            var r = new IdpResult()
            {
                Success = true,
                Message = "",
            };

            //เอา admin access token
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
            };
            var userToken = GetToken(form);
            if (userToken.Status != "Success")
            {
                r.Success = false;
                r.Message = $"Unable to get access token for delete user [{userToken.Message}]";
                return r;
            }

            // อ่านค่า UserId จาก UserName
            var userIdResult = GetUserIdByUsernameAsync(user.UserName!, userToken.Token.AccessToken).Result;
            if (!userIdResult.Success)
            {
                return userIdResult;
            }

            // ลบ user โดยใช้ UserId เป็น input
            var userId = userIdResult.UserId;
            var deleteResult = await DeleteUserAsync(userToken.Token.AccessToken, userId!);
            if (!deleteResult.Success)
            {
                return deleteResult;
            }

            return deleteResult;
        }

        public async Task<IdpResult> UpdateUserIdp(MUser user)
        {
            var r = new IdpResult()
            {
                Success = true,
                Message = "",
            };

            //เอา admin access token
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
            };
            var userToken = GetToken(form);
            if (userToken.Status != "Success")
            {
                r.Success = false;
                r.Message = $"Unable to get access token for password reset [{userToken.Message}]";
                return r;
            }

            // อ่านค่า UserId จาก UserName
            var userIdResult = GetUserIdByUsernameAsync(user.UserName!, userToken.Token.AccessToken).Result;
            if (!userIdResult.Success)
            {
                return userIdResult;
            }

            // เปลี่ยน user data โดยใช้ UserId เป็น input
            var userId = userIdResult.UserId;
            var updateResult = await UpdateUserAsync(user, userToken.Token.AccessToken, userId!);
            if (!updateResult.Success)
            {
                return updateResult;
            }

            return updateResult;
        }

        public async Task<IdpResult> ChangeUserPasswordIdp(MUpdatePassword password)
        {
            var r = new IdpResult()
            {
                Success = true,
                Message = "",
            };
            //Console.WriteLine($"@@@@@ [{password.UserName}] [{password.CurrentPassword}] [{password.NewPassword}] @@@@@");
            // เอา current password มา login ก่อนเพื่อดูว่าจะ login ได้มั้ย เพื่อมั่นใจว่าเค้ารู้ password เก่าจริง ๆ 
            var userLogin = new UserLogin()
            {
                UserName = password.UserName,
                Password = password.CurrentPassword,
            };
            var loginResult = Login(userLogin);
            if (loginResult.Status != "Success")
            {
                r.Success = false;
                r.Message = "Unable to login with current password!!!";
                return r;
            }

            //เอา admin access token
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
            };
            var userToken = GetToken(form);
            if (userToken.Status != "Success")
            {
                r.Success = false;
                r.Message = $"Unable to get access token for password reset [{userToken.Message}]";
                return r;
            }

            // อ่านค่า UserId จาก UserName
            var userIdResult = GetUserIdByUsernameAsync(password.UserName, userToken.Token.AccessToken).Result;
            if (!userIdResult.Success)
            {
                return userIdResult;
            }

            // เปลี่ยน password โดยใช้ UserId เป็น input
            var userId = userIdResult.UserId;
            var chagePasswordResult = await ChangeOwnPasswordAsync(password, userToken.Token.AccessToken, userId!);
            if (!chagePasswordResult.Success)
            {
                return chagePasswordResult;
            }

            // ต้อง logout session ของ user นั้นออกเพื่อบังคับให้ login ใหม่ (ใช้ refresh token เพื่อขอ access token ไม่ได้)
            var logoutResult = await LogoutUserAsync(userToken.Token.AccessToken, userId!);
            return logoutResult;
        }

        public async Task<IdpResult> ChangeForgotUserPasswordIdp(MUpdatePassword password)
        {
            var r = new IdpResult()
            {
                Success = true,
                Message = "",
            };

            //เอา admin access token
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
            };
            var userToken = GetToken(form);
            if (userToken.Status != "Success")
            {
                r.Success = false;
                r.Message = $"Unable to get access token for password reset [{userToken.Message}]";
                return r;
            }

            // อ่านค่า UserId จาก UserName
            var userIdResult = GetUserIdByUsernameAsync(password.UserName, userToken.Token.AccessToken).Result;
            if (!userIdResult.Success)
            {
                return userIdResult;
            }

            // เปลี่ยน password โดยใช้ UserId เป็น input
            var userId = userIdResult.UserId;
            var chagePasswordResult = await ChangeOwnPasswordAsync(password, userToken.Token.AccessToken, userId!);
            if (!chagePasswordResult.Success)
            {
                return chagePasswordResult;
            }

            // ต้อง logout session ของ user นั้นออกเพื่อบังคับให้ login ใหม่ (ใช้ refresh token เพื่อขอ access token ไม่ได้)
            var logoutResult = await LogoutUserAsync(userToken.Token.AccessToken, userId!);
            return logoutResult;
        }

        public async Task<IdpResult> UserLogoutIdp(string userName)
        {
            // คนเรียก function จะส่ง userName ที่ตรงกับใน accessToken มาให้เอง

            var r = new IdpResult()
            {
                Success = true,
                Message = "",
            };

            //เอา admin access token
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
            };
            var adminToken = GetToken(form);
            if (adminToken.Status != "Success")
            {
                r.Success = false;
                r.Message = $"Unable to get access token for user logout [{adminToken.Message}]";
                return r;
            }

            // อ่านค่า UserId จาก UserName
            var userIdResult = GetUserIdByUsernameAsync(userName, adminToken.Token.AccessToken).Result;
            if (!userIdResult.Success)
            {
                return userIdResult;
            }

            // logout โดยใช้ UserId เป็น input
            var userId = userIdResult.UserId;
            var logoutResult = await LogoutUserAsync(adminToken.Token.AccessToken, userId!);

            return logoutResult;
        }
    }
}
