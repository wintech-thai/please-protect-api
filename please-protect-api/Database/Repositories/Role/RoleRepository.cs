using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using LinqKit;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public RoleRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<MRole> GetRolesList(string rolesList)
        {
            var list = rolesList.Split(',').ToList();
            var arr = context!.Roles!.Where(p => list.Contains(p.RoleName!)).ToList();

            return arr;
        }

        private ExpressionStarter<MRole> RolePredicate(VMRole param)
        {
            var pd = PredicateBuilder.New<MRole>();

            pd = pd.And(p => p.RoleLevel!.Equals("ORGANIZATION"));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MRole>();
                fullTextPd = fullTextPd.Or(p => p.RoleName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RoleDefinition!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.RoleDescription!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public IEnumerable<MRole> GetRoles(VMRole param)
        {
            //Get all, no paging

            var predicate = RolePredicate(param!);
            var arr = context!.Roles!.Where(predicate)
                .OrderByDescending(e => e.RoleCreatedDate)
                .ToList();

            return arr;
        }
    }
}