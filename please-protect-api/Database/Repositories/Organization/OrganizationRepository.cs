using Its.PleaseProtect.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class OrganizationRepository : BaseRepository, IOrganizationRepository
    {
        public OrganizationRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public Task<MOrganization> GetOrganization()
        {
            var result = context!.Organizations!.Where(x => x.OrgCustomId!.Equals(orgId)).FirstOrDefaultAsync();
            return result!;
        }

        public MOrganizationUser AddUserToOrganization(MOrganizationUser user)
        {
            user.OrgCustomId = orgId;
            context!.OrganizationUsers!.Add(user);
            context.SaveChanges();

            return user;
        }

        public IEnumerable<MOrganizationUser> GetUserAllowedOrganization(string userName)
        {
            //var m = context!.OrganizationUsers!.Where(
            //    p => p!.UserName!.Equals(userName))
            //    .OrderByDescending(e => e.OrgCustomId)
            //    .ToList();

            var result =
                from ou in context!.OrganizationUsers
                where ou.UserName == userName
                orderby ou.OrgCustomId descending

                // JOIN กับ Organizations
                join o in context.Organizations!
                    on ou.OrgCustomId equals o.OrgCustomId into orgJoin
                from o in orgJoin.DefaultIfEmpty()

                // JOIN กับ Users
                join u in context.Users!
                    on ou.UserName equals u.UserName into userJoin
                from u in userJoin.DefaultIfEmpty()

                select new MOrganizationUser
                {
                    UserName = ou.UserName,
                    OrgCustomId = ou.OrgCustomId,
                    OrgDesc = o != null ? o.OrgDescription : null,
                    OrgName = o != null ? o.OrgName : null,
                    UserEmail = u != null ? u.UserEmail : null
                };

            return result.ToList();
        }

        public bool IsUserNameExist(string userName)
        {
            var cnt = context!.OrganizationUsers!.Where(
                    p => p!.UserName!.Equals(userName) && p!.OrgCustomId!.Equals(orgId)
                ).Count();

            return cnt >= 1;
        }

        public bool IsCustomOrgIdExist(string orgCustomId)
        {
            var cnt = context!.Organizations!.Where(
                    p => p!.OrgCustomId!.Equals(orgCustomId)
                ).Count();

            return cnt >= 1;
        }

        public MOrganizationUser GetUserInOrganization(string userName)
        {
            var m = context!.OrganizationUsers!.Where(
                p => p!.UserName!.Equals(userName) && p!.OrgCustomId!.Equals(orgId)).FirstOrDefault();

            return m!;
        }

        public async Task<MOrganization?> UpdateOrganization(MOrganization org)
        {
            var result = await context!.Organizations!.Where(x => x.OrgCustomId!.Equals(orgId)).FirstOrDefaultAsync();
            if (result != null)
            {
                result.OrgName = org.OrgName;
                result.OrgDescription = org.OrgDescription;
                result.Tags = org.Tags;
                result.Addresses = org.Addresses;
                result.Channels = org.Channels;
                result.LogoImagePath = org.LogoImagePath;

                await context.SaveChangesAsync();
            }

            return result;
        }

        public MOrganization AddOrganization(MOrganization org)
        {
            context!.Organizations!.Add(org);
            context.SaveChanges();

            return org;
        }
    }
}
