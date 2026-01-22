using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Services
{
    public class IoCService : BaseService, IIoCService
    {
        private readonly IIoCRepository? repository = null;

        public IoCService(IIoCRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVIoC> GetIoCById(string orgId, string iocId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVIoC()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(iocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"IoC ID [{iocId}] format is invalid";

                return r;
            }

            var result = await repository!.GetIoCById(iocId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"IoC ID [{iocId}] not found for the organization [{orgId}]";

                return r;
            }

            r.IoC = result;

            return r;
        }

        public async Task<MVIoC> DeleteIoCById(string orgId, string iocId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVIoC()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(iocId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"IoC ID [{iocId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteIoCById(iocId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"IoC ID [{iocId}] not found for the organization [{orgId}]";

                return r;
            }

            r.IoC = m;
            return r;
        }

        public async Task<List<MIoC>> GetIoCs(string orgId, VMIoC param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetIoCs(param);

            return result;
        }

        public async Task<int> GetIoCCount(string orgId, VMIoC param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetIoCCount(param);

            return result;
        }
    }
}
