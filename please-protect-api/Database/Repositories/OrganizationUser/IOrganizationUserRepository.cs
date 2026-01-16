using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.ViewsModels;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IOrganizationUserRepository
    {
        public void SetCustomOrgId(string customOrgId);
        public Task<MOrganizationUser> GetUserById(string orgUserId);
        public Task<MOrganizationUser> GetUserByIdLeftJoin(string orgUserId);
        public MOrganizationUser AddUser(MOrganizationUser user);
        public MOrganizationUser? DeleteUserById(string orgUserId);
        public IEnumerable<MOrganizationUser> GetUsers(VMOrganizationUser param);
        public IEnumerable<MOrganizationUser> GetUsersLeftJoin(VMOrganizationUser param);
        public int GetUserCount(VMOrganizationUser param);
        public int GetUserCountLeftJoin(VMOrganizationUser param);
        public MOrganizationUser? UpdateUserById(string orgUserId, MOrganizationUser user);
        public MOrganizationUser? UpdateUserStatusById(string orgUserId, string userId, string status);
        public MOrganizationUser? UpdateUserStatusById(string orgUserId, string status);
        public bool IsUserNameExist(string userName);
        public MOrganizationUser GetUserInOrganization(string userName);
    }
}
