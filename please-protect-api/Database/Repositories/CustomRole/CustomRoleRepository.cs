using LinqKit;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using System.Data.Entity;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class CustomRoleRepository : BaseRepository, ICustomRoleRepository
    {
        public CustomRoleRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsRoleNameExist(string roleName)
        {
            var exists = await context!.CustomRoles!.AsExpandable().AnyAsync(p => p!.RoleName!.Equals(roleName) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MCustomRole?> GetCustomRoleByName(string roleName)
        {
            var exists = await context!.CustomRoles!.AsExpandable().Where(p => p!.RoleName!.Equals(roleName) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<List<MCustomRole>> GetCustomRoles(VMCustomRole param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = CustomRolePredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.RoleCreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                r.RoleDefinition = "";
            }

            return result;
        }

        public async Task<int> GetCustomRoleCount(VMCustomRole param)
        {
            var predicate = CustomRolePredicate(param!);
            var result = await context!.CustomRoles!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MCustomRole?> GetCustomRoleById(string customRoleId)
        {
            Guid id = Guid.Parse(customRoleId);
            var u = await GetSelection().AsExpandable().Where(p => p!.RoleId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MCustomRole> GetSelection()
        {
            var query =
                from sca in context!.CustomRoles
                select new { sca };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MCustomRole
            {
                RoleId = x.sca.RoleId,
                OrgId = x.sca.OrgId,
                RoleName = x.sca.RoleName,
                RoleDefinition = x.sca.RoleDefinition,
                RoleDescription = x.sca.RoleDescription,
                RoleCreatedDate = x.sca.RoleCreatedDate,
                Tags = x.sca.Tags,
                Level = x.sca.Level,
            });
        }

        private ExpressionStarter<MCustomRole> CustomRolePredicate(VMCustomRole param)
        {
            var pd = PredicateBuilder.New<MCustomRole>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MCustomRole>();
                fullTextPd = fullTextPd.Or(p => p.RoleName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RoleDescription!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.Level != "") && (param.Level != null))
            {
                var levelPd = PredicateBuilder.New<MCustomRole>();
                levelPd = levelPd.Or(p => p.Level!.Equals(param.Level));

                pd = pd.And(levelPd);
            }

            return pd;
        }

        public async Task<MCustomRole> AddCustomRole(MCustomRole customRole)
        {
            customRole.OrgId = orgId;
            customRole.RoleCreatedDate = DateTime.UtcNow;

            await context!.CustomRoles!.AddAsync(customRole);
            await context.SaveChangesAsync();

            return customRole;
        }

        public async Task<MCustomRole?> DeleteCustomRoleById(string customRoleId)
        {
            Guid id = Guid.Parse(customRoleId);
            var existing = await context!.CustomRoles!.AsExpandable().Where(p => p!.RoleId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.CustomRoles!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MCustomRole?> UpdateCustomRoleById(string customRoleId, MCustomRole customRole)
        {
            Guid id = Guid.Parse(customRoleId);
            var existing = await context!.CustomRoles!.AsExpandable().Where(p => p!.RoleId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.RoleName = customRole.RoleName;
                existing.RoleDefinition = customRole.RoleDefinition;
                existing.Tags = customRole.Tags;
                existing.RoleDescription = customRole.RoleDescription;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}