using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IRoleRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public IEnumerable<MRole> GetRolesList(string rolesList);
        public IEnumerable<MRole> GetRoles(VMRole param);
    }
}
