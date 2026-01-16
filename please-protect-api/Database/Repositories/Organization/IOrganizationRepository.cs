using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IOrganizationRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MOrganization> GetOrganization();
        public Task<MOrganization?> UpdateOrganization(MOrganization org);
        public MOrganizationUser AddUserToOrganization(MOrganizationUser user);
        public bool IsUserNameExist(string userName);
        public bool IsCustomOrgIdExist(string orgCustomId);
        public MOrganizationUser GetUserInOrganization(string userName);
        public MOrganization AddOrganization(MOrganization org);
        public IEnumerable<MOrganizationUser> GetUserAllowedOrganization(string userName);
    }
}
