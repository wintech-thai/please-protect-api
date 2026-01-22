using LinqKit;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using System.Data.Entity;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class IoCRepository : BaseRepository, IIoCRepository
    {
        public IoCRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<List<MIoC>> GetIoCs(VMIoC param)
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

            var predicate = IoCPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.LastSeenDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                //r.MetaData = "";
            }

            return result;
        }

        public async Task<int> GetIoCCount(VMIoC param)
        {
            var predicate = IoCPredicate(param!);
            var result = await context!.Iocs!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MIoC?> GetIoCById(string iocId)
        {
            Guid id = Guid.Parse(iocId);
            var u = await GetSelection().AsExpandable().Where(p => p!.IocId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MIoC> GetSelection()
        {
            var query =
                from ioc in context!.Iocs
                select new { ioc };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MIoC
            {
                IocId = x.ioc.IocId,
                OrgId = x.ioc.OrgId,
                DataSet = x.ioc.DataSet,
                IocType = x.ioc.IocType,
                IocSubType = x.ioc.IocSubType,
                IocValue = x.ioc.IocValue,
                Tags = x.ioc.Tags,
                LastSeenDate = x.ioc.LastSeenDate,
            });
        }

        private ExpressionStarter<MIoC> IoCPredicate(VMIoC param)
        {
            var pd = PredicateBuilder.New<MIoC>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MIoC>();
                fullTextPd = fullTextPd.Or(p => p.DataSet!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.IocValue!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.IocType != "") && (param.IocType != null))
            {
                var typePd = PredicateBuilder.New<MIoC>();
                typePd = typePd.Or(p => p.IocType!.Equals(param.IocType));

                pd = pd.And(typePd);
            }

            return pd;
        }

        public async Task<MIoC?> DeleteIoCById(string iocId)
        {
            Guid id = Guid.Parse(iocId);
            var existing = await context!.Iocs!.AsExpandable().Where(p => p!.IocId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.Iocs!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }
    }
}