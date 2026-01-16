using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Services
{
    public interface IOrganizationService
    {
        public Task<MOrganization> GetOrganization(string orgId);
        public Task<MVOrganization> UpdateOrganization(string orgId, MOrganization org);
        public MVOrganizationUser AddUserToOrganization(string orgId, MOrganizationUser user);
        public bool IsUserNameExist(string orgId, string userName);
        public MVOrganizationUser VerifyUserInOrganization(string orgId, string userName);
        public MVOrganization AddOrganization(string orgId, MOrganization org);
        public IEnumerable<MOrganizationUser> GetUserAllowedOrganization(string userName);
        public bool IsOrgIdExist(string orgId);
        public MVPresignedUrl GetLogoImageUploadPresignedUrl(string orgId);
        public IEnumerable<MKeyValue> GetAllowChannelNames(string orgId);
        public IEnumerable<MKeyValue> GetAllowAddressTypeNames(string orgId);
    }
}
