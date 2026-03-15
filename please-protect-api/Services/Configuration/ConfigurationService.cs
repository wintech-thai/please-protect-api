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

        public async Task<MVConfiguration?> GetDomain(string orgId)
        {
            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Domain retrieved successfully"
            };

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetConfigurationByType("Domain");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Domain not found for the specified organization";
                return r;
            }

            r.Configuration = result;

            return r;
        }

        public async Task<MVConfiguration> SetDomain(string orgId, string domain)
        {
            repository!.SetCustomOrgId(orgId);
            
            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Domain retrieved successfully"
            };

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "Domain",
                ConfigValue = domain
            });

            r.Configuration = c;

            return r;
        }

        public async Task<MVConfiguration?> GetOrgShortName(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Organization short name retrieved successfully"
            };

            var result = await repository!.GetConfigurationByType("OrgShortName");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Organization short name not found for the specified organization";
                return r;
            }

            r.Configuration = result;

            return r;
        }

        public async Task<MVConfiguration> SetOrgShortName(string orgId, string shortName)
        {
            repository!.SetCustomOrgId(orgId);
            
            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Organization short name retrieved successfully"
            };

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "OrgShortName",
                ConfigValue = shortName
            });

            r.Configuration = c;

            return r;
        }

        public async Task<MVConfiguration?> GetOrgDescription(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Organization description retrieved successfully"
            };

            var result = await repository!.GetConfigurationByType("OrgDescription");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Organization description not found for the specified organization";
                return r;
            }

            r.Configuration = result;

            return r;
        }
        
        public async Task<MVConfiguration> SetOrgDescription(string orgId, string description)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Organization description updated successfully"
            };

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "OrgDescription",
                ConfigValue = description
            });

            r.Configuration = c;

            return r;
        }

        public async Task<MVConfiguration?> GetLogo(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Organization logo retrieved successfully"
            };

            var result = await repository!.GetConfigurationByType("Logo");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Organization logo not found for the specified organization";
                return r;
            }

            r.Configuration = result;

            return r;
        }

        public async Task<MVConfiguration> SetLogo(string orgId, string logoUrl)
        {
            repository!.SetCustomOrgId(orgId);
            
            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Logo retrieved successfully"
            };

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "Logo",
                ConfigValue = logoUrl
            });

            r.Configuration = c;

            return r;
        }

        public async Task<MVConfiguration?> GetCloudUrl(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Cloud URL retrieved successfully"
            };

            var result = await repository!.GetConfigurationByType("CloudUrl");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Cloud URL not found for the specified organization";
                return r;
            }

            r.Configuration = result;

            return r;
        }

        public async Task<MVConfiguration> SetCloudUrl(string orgId, string cloudUrl)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Cloud URL updated successfully"
            };

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "CloudUrl",
                ConfigValue = cloudUrl
            });

            r.Configuration = c;

            return r;
        }

        public async Task<MVConfiguration?> GetCloudConnectKey(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Cloud connect key retrieved successfully"
            };

            var result = await repository!.GetConfigurationByType("CloudConnectKey");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Cloud connect key not found for the specified organization";
                return r;
            }

            r.Configuration = result;

            return r;
        }

        public async Task<MVConfiguration> SetCloudConnectKey(string orgId, string cloudConnectKey)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Cloud connect key updated successfully"
            };

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "CloudConnectKey",
                ConfigValue = cloudConnectKey
            });

            r.Configuration = c;

            return r;
        }

        public async Task<MVConfiguration?> GetCloudConnectFlag(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Cloud connect flag retrieved successfully"
            };

            var result = await repository!.GetConfigurationByType("CloudConnectFlag");

            if (result == null)
            {
                r.Status = "NOT_FOUND";
                r.Description = "Cloud connect flag not found for the specified organization";
                return r;
            }

            r.Configuration = result;

            return r;
        }

        public async Task<MVConfiguration> SetCloudConnectFlag(string orgId, string cloudConnectFlag)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVConfiguration() 
            { 
                Status = "SUCCESS",
                Description = "Cloud connect flag updated successfully"
            };

            var c = await repository!.UpsertConfiguration(new MConfiguration()
            {
                ConfigType = "CloudConnectFlag",
                ConfigValue = cloudConnectFlag.ToString()
            });

            r.Configuration = c;

            return r;
        }
    }
}
