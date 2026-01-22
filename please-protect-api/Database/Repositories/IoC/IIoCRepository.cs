using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IIoCRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<List<MIoC>> GetIoCs(VMIoC param);
        public Task<int> GetIoCCount(VMIoC param);
        public Task<MIoC?> GetIoCById(string iocId);
        public Task<MIoC?> DeleteIoCById(string iocId);
    }
}
