using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Services
{
    public interface IConfigurationService
    {
        public MVEsConfig GetEsConfig(string orgId);

        public Task<MConfiguration?> GetDomain(string orgId);
        public Task<MConfiguration> SetDomain(string orgId, string domain);

        public Task<MConfiguration?> GetOrgShortName(string orgId);
        public Task<MConfiguration> SetOrgShortName(string orgId, string shortName);

        public Task<MConfiguration?> GetLogo(string orgId);
        public Task<MConfiguration> SetLogo(string orgId, string logoUrl);
    }
}
