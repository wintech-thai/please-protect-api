using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IJobRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public MJob AddJob(MJob cycle);
        public int GetJobCount(VMJob param);
        public IEnumerable<MJob> GetJobs(VMJob param);
        public MJob GetJobById(string cycleId);
        public MJob? DeleteJobById(string jobId);
    }
}
