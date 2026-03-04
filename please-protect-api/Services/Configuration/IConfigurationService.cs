using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Services
{
    public interface IConfigurationService
    {
        public Task<MVConfiguration?> GetDomain(string orgId);
        public Task<MVConfiguration> SetDomain(string orgId, string domain);

        public Task<MVConfiguration?> GetOrgShortName(string orgId);
        public Task<MVConfiguration> SetOrgShortName(string orgId, string shortName);

        public Task<MVConfiguration?> GetLogo(string orgId);
        public Task<MVConfiguration> SetLogo(string orgId, string logoUrl);
    }
}
