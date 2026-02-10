using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class OrganizationUserRepository : BaseRepository, IOrganizationUserRepository
    {
        public OrganizationUserRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public Task<MOrganizationUser> GetUserById(string orgUserId)
        {
            Guid id = Guid.Parse(orgUserId);

            var result = context!.OrganizationUsers!
                .Join(context!.Users!,
                    ou => ou.UserName,
                    u => u.UserName,
                    (ou, u) => new MOrganizationUser
                    {
                        OrgUserId = ou.OrgUserId,
                        OrgCustomId = ou.OrgCustomId,
                        UserId = ou.UserId,
                        UserName = ou.UserName,
                        RolesList = ou.RolesList,
                        CreatedDate = ou.CreatedDate,
                        UserEmail = u.UserEmail,
                        Tags = ou.Tags,
                    })
                .Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId!.Equals(id)).FirstOrDefaultAsync();

            return result!;
        }

        public Task<MOrganizationUser> GetUserByIdLeftJoin(string orgUserId)
        {
            Guid id = Guid.Parse(orgUserId);
            var result = GetSelection().Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId.Equals(id)).FirstOrDefaultAsync();

            return result!;
        }

        public MOrganizationUser AddUser(MOrganizationUser user)
        {
            user.OrgUserId = Guid.NewGuid();
            user.CreatedDate = DateTime.UtcNow;
            user.OrgCustomId = orgId;

            context!.OrganizationUsers!.Add(user);
            context.SaveChanges();

            return user;
        }

        public MOrganizationUser? DeleteUserById(string orgUserId)
        {
            Guid id = Guid.Parse(orgUserId);

            var r = context!.OrganizationUsers!.Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId.Equals(id)).FirstOrDefault();
            if (r != null)
            {
                context!.OrganizationUsers!.Remove(r);
                context.SaveChanges();
            }

            return r;
        }

        public IEnumerable<MOrganizationUser> GetUsers(VMOrganizationUser param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = UserPredicate(param!);

            var arr = context!.OrganizationUsers!
                .Join(context!.Users!,
                    ou => ou.UserName,
                    u => u.UserName,
                    (ou, u) => new MOrganizationUser
                    {
                        OrgUserId = ou.OrgUserId,
                        OrgCustomId = ou.OrgCustomId,
                        UserId = ou.UserId,
                        UserName = ou.UserName,
                        RolesList = ou.RolesList,
                        CreatedDate = ou.CreatedDate,
                        UserEmail = u.UserEmail,
                        Tags = ou.Tags,
                    })
                .AsQueryable()
                .AsExpandable()
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        public MOrganizationUser GetUserInOrganization(string userName)
        {
            var m = GetSelection().Where(
                p => p!.UserName!.Equals(userName) && p!.OrgCustomId!.Equals(orgId)).FirstOrDefault();

            return m!;
        }

        public IQueryable<MOrganizationUser> GetSelection()
        {
            var query =
                from ou in context!.OrganizationUsers

                join usr in context.Users!
                    on ou.UserId equals usr.UserId.ToString() into joinedUser
                from user in joinedUser.DefaultIfEmpty()

                join cr in context.CustomRoles!
                    on ou.CustomRoleId equals cr.RoleId.ToString() into joinedRole
                from role in joinedRole.DefaultIfEmpty()

                select new { ou, user, role };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MOrganizationUser
            {
                OrgUserId = x.ou.OrgUserId,
                OrgCustomId = x.ou.OrgCustomId,
                UserId = x.ou.UserId,
                UserName = x.ou.UserName,
                RolesList = x.ou.RolesList,
                CreatedDate = x.ou.CreatedDate,
                UserEmail = x.user.UserEmail,
                TmpUserEmail = x.ou.TmpUserEmail,
                UserStatus = x.ou.UserStatus,
                PreviousUserStatus = x.ou.PreviousUserStatus,
                InvitedDate = x.ou.InvitedDate,
                IsOrgInitialUser = x.ou.IsOrgInitialUser,
                Tags = x.ou.Tags,
                CustomRoleId = x.ou.CustomRoleId,
                CustomRoleName = x.role.RoleName,
                CustomRoleDesc = x.role.RoleDescription,
            });
        }

        public IEnumerable<MOrganizationUser> GetUsersLeftJoin(VMOrganizationUser param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset - 1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = UserPredicate(param!);

            var arr = GetSelection()
                .Where(predicate)
                .OrderByDescending(e => e.CreatedDate)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return arr;
        }

        private ExpressionStarter<MOrganizationUser> UserPredicate(VMOrganizationUser param)
        {
            var pd = PredicateBuilder.New<MOrganizationUser>();

            pd = pd.And(p => p.OrgCustomId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MOrganizationUser>();
                fullTextPd = fullTextPd.Or(p => p.UserEmail!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.UserName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public int GetUserCount(VMOrganizationUser param)
        {
            var predicate = UserPredicate(param);
            var cnt = context!.OrganizationUsers!.Where(predicate).Count();

            return cnt;
        }

        public int GetUserCountLeftJoin(VMOrganizationUser param)
        {
            var predicate = UserPredicate(param);

            var cnt = GetSelection()
            .Where(predicate)
            .Count();

            return cnt;
        }

        public MOrganizationUser? UpdateUserById(string orgUserId, MOrganizationUser user)
        {
            Guid id = Guid.Parse(orgUserId);
            var result = context!.OrganizationUsers!.Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.RolesList = user.RolesList;
                result.CustomRoleId = user.CustomRoleId;
                result.Tags = user.Tags;

                context!.SaveChanges();
            }

            return result!;
        }

        public MOrganizationUser? UpdateUserStatusById(string orgUserId, string userId, string status)
        {
            Guid id = Guid.Parse(orgUserId);
            var result = context!.OrganizationUsers!.Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.PreviousUserStatus = result.UserStatus;
                result.UserStatus = status;
                result.UserId = userId;

                context!.SaveChanges();
            }

            return result!;
        }

        public MOrganizationUser? UpdateUserStatusById(string orgUserId, string status)
        {
            Guid id = Guid.Parse(orgUserId);
            var result = context!.OrganizationUsers!.Where(x => x.OrgCustomId!.Equals(orgId) && x.OrgUserId!.Equals(id)).FirstOrDefault();

            if (result != null)
            {
                result.PreviousUserStatus = result.UserStatus;
                result.UserStatus = status;

                context!.SaveChanges();
            }

            return result!;
        }

        public bool IsUserNameExist(string userName)
        {
            var cnt = context!.OrganizationUsers!.Where(p => p.OrgCustomId!.Equals(orgId) && p!.UserName!.Equals(userName)).Count();
            return cnt >= 1;
        }
    }
}