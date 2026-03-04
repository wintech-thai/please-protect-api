using LinqKit;
using Its.PleaseProtect.Api.Models;
using System.Data.Entity;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class ConfigurationRepository : BaseRepository, IConfigurationRepository
    {
        public ConfigurationRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MConfiguration?> GetConfigurationByType(string configType)
        {
            var u = await context!.Configurations!.AsExpandable()
                .Where(p => p!.ConfigType!.Equals(configType) && p!.OrgId!.Equals(orgId))
                .FirstOrDefaultAsync();

            return u;
        }

        public async Task<MConfiguration> UpsertConfiguration(MConfiguration config)
        {
            var existing = await GetConfigurationByType(config.ConfigType!);
            if (existing != null)
            {
                // Update existing configuration
                existing.ConfigValue = config.ConfigValue;
                existing.CreatedDate = DateTime.UtcNow;
            }
            else
            {
                // Add new configuration
                config.OrgId = orgId;
                config.CreatedDate = DateTime.UtcNow;
                context!.Configurations!.Add(config);
            }

            await context!.SaveChangesAsync();
            return config;
        }
    }
}