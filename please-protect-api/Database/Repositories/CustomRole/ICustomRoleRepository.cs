using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface ICustomRoleRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsRoleNameExist(string roleName);
        public Task<List<MCustomRole>> GetCustomRoles(VMCustomRole param);
        public Task<int> GetCustomRoleCount(VMCustomRole param);
        public Task<MCustomRole?> GetCustomRoleById(string customRoleId);
        public Task<MCustomRole> AddCustomRole(MCustomRole customRole);
        public Task<MCustomRole?> DeleteCustomRoleById(string customRoleId);
        public Task<MCustomRole?> UpdateCustomRoleById(string customRoleId, MCustomRole customRole);
        public Task<MCustomRole?> GetCustomRoleByName(string roleName);
    }
}
