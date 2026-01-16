using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.Utils;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Services
{
    public class ApiKeyService : BaseService, IApiKeyService
    {
        private readonly IApiKeyRepository? repository = null;
        private DateTime compareDate = DateTime.Now;
        private readonly IRedisHelper _redis;

        public ApiKeyService(
            IApiKeyRepository repo,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public void SetCompareDate(DateTime dtm)
        {
            //For unit testing injection
            compareDate = dtm;
        }

        public Task<MApiKey> GetApiKey(string orgId, string apiKey)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKey(apiKey);

            return result;
        }

        public Task<MApiKey> GetApiKeyByName(string orgId, string keyName)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKeyByName(keyName);

            return result;
        }

        public MVApiKey VerifyApiKey(string orgId, string apiKey)
        {
            repository!.SetCustomOrgId(orgId);
            var m = repository!.GetApiKey(apiKey).Result;

            var status = "OK";
            var description = "SUCCESS";

            if (m == null)
            {
                status = "NOTFOUND";
                description = $"API key not found for the organization [{orgId}]";
            }
            else if ((m.KeyExpiredDate != null) && (DateTime.Compare(compareDate, (DateTime)m.KeyExpiredDate!) > 0))
            {
                status = "EXPIRED";
                description = $"API key for the organization is expire [{orgId}] since [{m.KeyExpiredDate}]";
            }
            else if ((m.KeyStatus != null) && m.KeyStatus!.Equals("Disabled"))
            {
                status = "DISABLED";
                description = $"API key for the organization is disabled [{orgId}]";
            }
            
            var mv = new MVApiKey()
            {
                ApiKey = m,
                Status = status,
                Description = description,
            };

            return mv;
        }

        public MVApiKey? AddApiKey(string orgId, MApiKey apiKey)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVApiKey();
            var t = repository!.GetApiKey(apiKey.ApiKey!);
            var m = t.Result;

            if (m != null)
            {
                r.Status = "KEY_DUPLICATE";
                r.Description = "API Key is duplicate";

                return r;
            }

            t = repository!.GetApiKeyByName(apiKey.KeyName!);
            m = t.Result;
            if (m != null)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = "API Key name is duplicate";

                return r;
            }

            apiKey.RolesList = string.Join(",", apiKey.Roles ?? []);
            apiKey.KeyStatus = "Active"; //Default status
            apiKey.ApiKey = Guid.NewGuid().ToString(); //ให้ return ออกไปด้วยเพื่อให้ user ใช้งานได้เลย
            var result = repository!.AddApiKey(apiKey);

            r.Status = "OK";
            r.Description = "Success";
            r.ApiKey = result;

            r.ApiKey.RolesList = "";

            return r;
        }

        private void DeleteApiKeyCache(string orgId, string apiKey)
        {
            var key = $"#{orgId}:VerifyKey:#{apiKey}";
            _redis.DeleteAsync(key);
        }

        public MVApiKey? DeleteApiKeyById(string orgId, string keyId)
        {
            var r = new MVApiKey()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(keyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Key ID [{keyId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteApiKeyById(keyId);

            r.ApiKey = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Key ID [{keyId}] not found for the organization [{orgId}]";
            }
            else
            {
                DeleteApiKeyCache(orgId, r.ApiKey!.ApiKey!);
                r.ApiKey!.ApiKey = "";
            }

            return r;
        }

        public IEnumerable<MApiKey> GetApiKeys(string orgId, VMApiKey param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKeys(param);

            foreach (var key in result)
            {
                //เพื่อไม่ให้ return ค่า ApiKey กลับไป
                key.ApiKey = "";
            }

            return result;
        }

        public int GetApiKeyCount(string orgId, VMApiKey param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetApiKeyCount(param);

            return result;
        }

        public MVApiKey? UpdateApiKeyById(string orgId, string keyId, MApiKey apiKey)
        {
            var r = new MVApiKey()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            apiKey.RolesList = string.Join(",", apiKey.Roles ?? []);
            var result = repository!.UpdateApiKeyById(keyId, apiKey);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Key ID [{keyId}] not found for the organization [{orgId}]";

                return r;
            }

            DeleteApiKeyCache(orgId, result.ApiKey!);

            r.ApiKey = result;
            r.ApiKey.RolesList = "";
            r.ApiKey.ApiKey = "";

            return r;
        }

        public MVApiKey? UpdateApiKeyStatusById(string orgId, string keyId, string status)
        {
            var r = new MVApiKey()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            var result = repository!.UpdateApiKeyStatusById(keyId, status);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Key ID [{keyId}] not found for the organization [{orgId}]";

                return r;
            }

            DeleteApiKeyCache(orgId, result.ApiKey!);

            r.ApiKey = result;
            r.ApiKey.RolesList = "";
            r.ApiKey.ApiKey = "";

            return r;
        }

        public async Task<MVApiKey> GetApiKeyById(string orgId, string keyId)
        {
            var r = new MVApiKey()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(keyId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Key ID [{keyId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var key = await repository!.GetApiKeyById(keyId);

            if (key == null)
            {
                r.Status = "KEY_NOTFOUND";
                r.Description = $"Key ID [{keyId}] not found!!!";

                return r;
            }

            if (!string.IsNullOrEmpty(key.RolesList))
            {
                key.Roles = [.. key.RolesList.Split(',')];
            }
            key.RolesList = "";
            key.ApiKey = ""; //ไม่ต้อง return ค่า api key กลับไป

            r.ApiKey = key;

            return r;
        }
    }
}
