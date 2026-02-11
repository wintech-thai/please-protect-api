using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public MUser AddUser(MUser user)
        {
            context!.Users!.Add(user);
            context.SaveChanges();

            return user;
        }

        public IEnumerable<MUser> GetUsers()
        {
            //Get All, do this query below will be easier for mocked unit testing
            var arr = context!.Users!.Where(p => !p.UserName!.Equals(null)).ToList();
            return arr;
        }

        public bool IsEmailExist(string email)
        {
            var cnt = context!.Users!.Where(p => p!.UserEmail!.Equals(email)).Count();
            return cnt >= 1;
        }

        public bool IsUserNameExist(string userName)
        {
            var cnt = context!.Users!.Where(p => p!.UserName!.Equals(userName)).Count();
            return cnt >= 1;
        }

        public bool IsUserIdExist(string userId)
        {
            try
            {
                Guid id = Guid.Parse(userId);
                var cnt = context!.Users!.Where(p => p!.UserId!.Equals(id)).Count();
                return cnt >= 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public MUser GetUserByName(string userName)
        {
            var u = context!.Users!.Where(p => p!.UserName!.Equals(userName)).FirstOrDefault();
            return u!;
        }

        public MUser GetUserByUserName(string userName)
        {
            //เปลี่ยนใช้ชื่อให้สือความหมายมากขึ้น
            var u = context!.Users!.Where(p => p!.UserName!.Equals(userName)).FirstOrDefault();
            return u!;
        }

        public MUser UpdateUserByUserName(string userName, MUser user)
        {
            var u = context!.Users!.Where(p => p!.UserName!.Equals(userName)).FirstOrDefault();
            if (u != null)
            {
                u.Name = user.Name;
                u.LastName = user.LastName;
                u.SecondaryEmail = user.SecondaryEmail;
                u.PhoneNumber = user.PhoneNumber;

                context!.SaveChanges();
            }

            return u!;
        }

        public MUser DeleteUserById(string userId)
        {
            Guid id = Guid.Parse(userId);

            var u = context!.Users!.Where(p => p!.UserId!.Equals(id)).FirstOrDefault();
            if (u != null)
            {
                context!.Users!.Remove(u);
                context!.SaveChanges();
            }

            return u!;
        }

        public MUser GetUserByEmail(string email)
        {
            var u = context!.Users!.Where(p => p!.UserEmail!.Equals(email)).FirstOrDefault();
            return u!;
        }
    }
}