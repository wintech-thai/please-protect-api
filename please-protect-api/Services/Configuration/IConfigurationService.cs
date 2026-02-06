using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Services
{
    public interface IConfigurationService
    {
        public MVEsConfig GetEsConfig(string orgId);
    }
}
