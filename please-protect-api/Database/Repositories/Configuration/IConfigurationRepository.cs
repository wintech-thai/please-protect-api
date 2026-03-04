using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IConfigurationRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<MConfiguration?> GetConfigurationByType(string configType);
        public Task<MConfiguration> UpsertConfiguration(MConfiguration config);
    }
}
