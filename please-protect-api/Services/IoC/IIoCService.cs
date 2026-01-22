using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Services
{
    public interface IIoCService
    {
        public Task<MVIoC> GetIoCById(string orgId, string iocId);
        public Task<MVIoC> DeleteIoCById(string orgId, string iocId);
        public Task<List<MIoC>> GetIoCs(string orgId, VMIoC param);
        public Task<int> GetIoCCount(string orgId, VMIoC param);
    }
}
