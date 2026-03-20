using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.Services
{
    public interface IApplicationService
    {
        public Task<List<MApplication>> GetApplications(string orgId);
    }
}
