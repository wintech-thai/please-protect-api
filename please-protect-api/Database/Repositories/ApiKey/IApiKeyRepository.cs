using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IApiKeyRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MApiKey> GetApiKey(string apiKey);
        public Task<MApiKey> GetApiKeyByName(string keyName);
        public MApiKey AddApiKey(MApiKey apiKey);
        public MApiKey? DeleteApiKeyById(string keyId);
        public IEnumerable<MApiKey> GetApiKeys(VMApiKey param);

        public Task<MApiKey> GetApiKeyById(string keyId);
        public int GetApiKeyCount(VMApiKey param);
        public MApiKey? UpdateApiKeyById(string keyId, MApiKey apiKey);
        public MApiKey? UpdateApiKeyStatusById(string keyId, string status);
    }
}
