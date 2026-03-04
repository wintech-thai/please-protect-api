using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Services
{
    public class ConfigurationService : BaseService, IConfigurationService
    {
        private readonly IConfigurationRepository? repository = null;

        public ConfigurationService(IConfigurationRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MConfiguration?> GetDomain(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetConfigurationByType("Domain");
        }

        public async Task<MConfiguration> SetDomain(string orgId, string domain)
        {
            repository!.SetCustomOrgId(orgId);

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "Domain",
                ConfigValue = domain
            });

            return c;
        }

        public async Task<MConfiguration?> GetOrgShortName(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetConfigurationByType("OrgShortName");
        }

        public async Task<MConfiguration> SetOrgShortName(string orgId, string shortName)
        {
            repository!.SetCustomOrgId(orgId);

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "OrgShortName",
                ConfigValue = shortName
            });

            return c;
        }


        public async Task<MConfiguration?> GetLogo(string orgId)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetConfigurationByType("Logo");
        }

        public async Task<MConfiguration> SetLogo(string orgId, string logoUrl)
        {
            repository!.SetCustomOrgId(orgId);

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "Logo",
                ConfigValue = logoUrl
            });

            return c;
        }


        public MVEsConfig GetEsConfig(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVEsConfig()
            {
                Status = "OK",
                Description = "Success"
            };

            var url = Environment.GetEnvironmentVariable("ES_URL");
            if (string.IsNullOrEmpty(url))
            {
                r.Status = "ES_URL_MISSING";
                r.Description = "Elasticsearch env variable [ES_URL] is not configured";

                return r;
            }

            var user = Environment.GetEnvironmentVariable("ES_USER");
            if (string.IsNullOrEmpty(user))
            {
                r.Status = "ES_USER_MISSING";
                r.Description = "Elasticsearch env variable [ES_USER] is not configured";

                return r;
            }

            var password = Environment.GetEnvironmentVariable("ES_PASSWORD");
            if (string.IsNullOrEmpty(password))
            {
                r.Status = "ES_PASSWORD_MISSING";
                r.Description = "Elasticsearch env variable [ES_PASSWORD] is not configured";

                return r;
            }

            var indexPattern = Environment.GetEnvironmentVariable("ES_INDEX_PATTERN");
            if (string.IsNullOrEmpty(indexPattern))
            {
                r.Status = "ES_INDEX_PATTERN_MISSING";
                r.Description = "Elasticsearch env variable [ES_INDEX_PATTERN] is not configured";

                return r;
            }

            var cfg = new MEsConfig()
            {
                ApiEndpoint = url,
                User = user,
                Password = password,
                IndexPattern = indexPattern
            };

            r.EsConfig = cfg;
            return r;
        }
    }
}
