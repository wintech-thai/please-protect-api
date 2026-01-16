using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Services
{
    public interface IUserService
    {
        public MVUser AddUser(string orgId, MUser user);
        public IEnumerable<MUser> GetUsers(string orgId);
        public bool IsEmailExist(string orgId, string email);
        public bool IsUserNameExist(string orgId, string userName);
        public bool IsUserIdExist(string orgId, string userId);
        public MUser GetUserByName(string orgId, string userName);
        public MUser GetUserByEmail(string orgId, string email);
        public MVUpdatePassword UpdatePassword(string userName, MUpdatePassword password);
        public MVLogout UserLogout(string userName);

        //ไม่ใช้ orgId
        public MVUser GetUserByUserName(string userName);
        public MVUser UpdateUserByUserName(string userName, MUser user);
    }
}
