using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Services
{
    public interface IApplicationService
    {
        public Task<List<MApplication>> GetApplications(string orgId, GitUtil git, bool withCleanup);
        public Task<string> GetCurrentAppDefaultConfig(string orgId, GitUtil git, string appName);
        public Task<string> GetCurrentAppCustomConfig(string orgId, GitUtil git, string appName);
        public Task<string> GetDraftAppCustomConfig(string orgId, GitUtil git, string appName);
        public Task<string> SaveDraftAppCustomConfig(string orgId, GitUtil git, string appName, string content);
        public Task<string> MergeDraftAppCustomConfig(string orgId, GitUtil git, string appName);
    }
}
