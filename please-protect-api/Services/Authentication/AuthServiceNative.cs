
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Its.PleaseProtect.Api.Services
{
    public class AuthServiceNative : BaseService, IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRedisHelper _redis;
        private readonly int _tokenMinExpire = 10;
        private readonly int _freshTokenDayExpire = 2;
        private readonly string _issuer = "devhub";
        private readonly string _audient = "web";
    
        public AuthServiceNative(
            UserManager<IdentityUser> userManager, 
            IRedisHelper redis) : base()
        {
            _userManager = userManager;
            _redis = redis;
        }

        public SecurityToken ValidateAccessToken(string accessToken, JwtSecurityTokenHandler tokenHandler)
        {
            var secret = GetSignKeySync();

            var key = new SymmetricSecurityKey(
                Convert.FromBase64String(secret)
            );

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,

                ValidateAudience = true,
                ValidAudience = _audient,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ClockSkew = TimeSpan.Zero
            };

            var _ = tokenHandler.ValidateToken(
                accessToken,
                validationParameters,
                out SecurityToken validatedToken
            );

            return validatedToken;
        }

        private string GetSignKeySync()
        {
            var t = GetSignKey();
            return t.Result;
        }

        private async Task<string> GetSignKey()
        {
            var cacheKey = CacheHelper.CreateJwtSignKey();
            var secret = await _redis.GetAsync(cacheKey);

            if (secret == null)
            {
                //ไม่มี ให้สร้างแล้วใส่ cache ไว้
                secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                await _redis.SetAsync(cacheKey, secret, null); //Not expire
            }

            return secret;
        }

        private async Task<KeycloakTokenResponse> GenerateTokenAsync(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new("preferred_username", user.UserName ?? ""),
                new("name", user.UserName ?? ""),
                new("given_name", user.UserName ?? ""),
                new("family_name", user.UserName ?? ""),
                new("email", user.Email ?? ""),
                new("sub", user.Id)
            };

            var secret = await GetSignKey();
            var key = new SymmetricSecurityKey(Convert.FromBase64String(secret));

            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audient,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_tokenMinExpire),
                signingCredentials: credential
            );

            var refreshToken = Guid.NewGuid().ToString("N");
            var refreshData = new RefreshTokenData
            {
                UserId = user.Id,
                UserName = user.UserName!,
                ExpireAt = DateTime.UtcNow.AddDays(_freshTokenDayExpire)
            };

            var refreshTokenKey = CacheHelper.CreateRefreshTokenKey(refreshToken);
            await _redis.SetObjectAsync<RefreshTokenData>(refreshTokenKey, refreshData, TimeSpan.FromDays(_freshTokenDayExpire));

            return new KeycloakTokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = refreshToken,

                ExpiresIn = _tokenMinExpire * 60,
                RefreshExpiresIn = 0,
                TokenType = "Bearer"
            };
        }

        public UserToken Login(UserLogin userLogin)
        {
            var u = LoginV2(userLogin);
            return u.Result;
        }

        public async Task<UserToken> LoginV2(UserLogin userLogin)
        {
            var userToken = new UserToken()
            {
                Status = "Success",
                Message = "Success",
            };

            var user = await _userManager.FindByNameAsync(userLogin.UserName);

            if (user == null)
            {
                userToken.Status = "INVALID_LOGIN1";
                userToken.Message = "Invalid username or password";
                return userToken;
            }

            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                userLogin.Password
            );

            if (!passwordValid)
            {
                userToken.Status = "INVALID_LOGIN2";
                userToken.Message = "Invalid username or password";
                return userToken;
            }

            var token = await GenerateTokenAsync(user);
            userToken.UserName = user.UserName!;
            userToken.Token = token;

            return userToken;
        }

        public UserToken RefreshToken(string refreshToken)
        {
            var t = RefreshTokenV2(refreshToken);
            return t.Result;
        }

        public async Task<UserToken> RefreshTokenV2(string token)
        {
            var userToken = new UserToken()
            {
                Status = "Success",
                Message = "Success",
            };

            var refreshTokenKey = CacheHelper.CreateRefreshTokenKey(token);
            var refreshData  = await _redis.GetObjectAsync<RefreshTokenData>(refreshTokenKey);

            if (refreshData  == null)
            {
                userToken.Status = "REFRESH_TOKEN_NOT_FOUND";
                userToken.Message = "Invalid refresh token";
                return userToken;
            }

            var user = await _userManager.FindByIdAsync(refreshData.UserId);
            if (user == null)
            {
                userToken.Status = "USER_NOT_FOUND";
                userToken.Message = $"User with ID [{refreshData.UserId}] not found!!!";
                return userToken;
            }

            userToken.UserName = user.UserName!;
            //ตัว GenerateTokenAsync จะให้ refresh token ใหม่มาด้วย
            userToken.Token = await GenerateTokenAsync(user);

            //ลบ refresh token ของเดิม
            await _redis.DeleteAsync(refreshTokenKey);

            return userToken;
        }

        public async Task<IdpResult> AddUserToIDP(MOrganizeRegistration orgUser)
        {
            var user = new IdentityUser
            {
                UserName = orgUser.UserName,
                Email = orgUser.Email,
            };

            var result = await _userManager.CreateAsync(
                user,
                orgUser.UserInitialPassword
            );

            var t = new IdpResult()
            {
                Success = true,
                Message = $"User [{orgUser.UserName}] [{orgUser.Email}] created successfully!",
            };

            if (!result.Succeeded)
            {
                var msg = string.Join(",", result.Errors.Select(x => x.Description));
                
                t.Success = false;
                t.Message = msg;
            }

            return t;
        }

        public async Task<IdpResult> UpdateUserIdp(MUser user)
        {
            var t = new IdpResult()
            {
                Success = true,
                Message = $"User [{user.UserName}] [{user.UserEmail}] updated successfully!",
            };

            var u = await _userManager.FindByNameAsync(user.UserName!);
            if (u == null)
            {
                t.Success = false;
                t.Message = $"User [{user.UserName}] not found!!!";
                return t;
            }

            u.Email = user.UserEmail;
            u.PhoneNumber = user.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(u);
            if (!updateResult.Succeeded)
            {
                var msg = string.Join(",", updateResult.Errors.Select(x => x.Description));

                t.Success = false;
                t.Message = msg;

                return t;
            }

            return t;
        }

        public async Task<IdpResult> ChangeUserPasswordIdp(MUpdatePassword password)
        {
            var t = new IdpResult()
            {
                Success = true,
                Message = $"Password for user [{password.UserName}] updated successfully!",
            };

            var u = await _userManager.FindByNameAsync(password.UserName!);
            if (u == null)
            {
                t.Success = false;
                t.Message = $"User [{password.UserName}] not found!!!";
                return t;
            }

            var updateResult = await _userManager.ChangePasswordAsync(u, password.CurrentPassword, password.NewPassword);
            if (!updateResult.Succeeded)
            {
                var msg = string.Join(",", updateResult.Errors.Select(x => x.Description));

                t.Success = false;
                t.Message = msg;

                return t;
            }

            return t;
        }

        public async Task<IdpResult> ChangeForgotUserPasswordIdp(MUpdatePassword password)
        {
            var t = new IdpResult()
            {
                Success = true,
                Message = $"Password for user [{password.UserName}] updated successfully!",
            };

            var u = await _userManager.FindByNameAsync(password.UserName!);
            if (u == null)
            {
                t.Success = false;
                t.Message = $"User [{password.UserName}] not found!!!";
                return t;
            }

            // generate reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(u);

            var resetResult = await _userManager.ResetPasswordAsync(
                u,
                resetToken,
                password.NewPassword
            );

            if (!resetResult.Succeeded)
            {
                var msg = string.Join(",", resetResult.Errors.Select(x => x.Description));

                t.Success = false;
                t.Message = msg;

                return t;
            }

            return t;
        }

        public async Task<IdpResult> UserLogoutIdp(string userName)
        {
            var t = new IdpResult()
            {
                Success = true,
                Message = "Success",
            };

            //ไม่ให้ warning
            await Task.CompletedTask;

            return t;
        }

        public async Task<IdpResult> DeleteUserIdp(MUser user)
        {
            var r = new IdpResult()
            {
                Success = true,
                Message = "",
            };

            await Task.CompletedTask;

            return r;
        }
    }
}
