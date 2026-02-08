using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Services
{
    public class ConfigurationService : BaseService, IConfigurationService
    {
        private readonly IIoCRepository? repository = null;

        public ConfigurationService(IIoCRepository repo) : base()
        {
            repository = repo;
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
