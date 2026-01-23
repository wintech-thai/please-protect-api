using LinqKit;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using System.Data.Entity;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class SubnetRepository : BaseRepository, ISubnetRepository
    {
        public SubnetRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsSubnetNameExist(string name)
        {
            var exists = await context!.Subnets!.AsExpandable().AnyAsync(p => p!.Name!.Equals(name) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<bool> IsCidrExist(string cidr)
        {
            var exists = await context!.Subnets!.AsExpandable().AnyAsync(p => p!.Cidr!.Equals(cidr) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MSubnet?> GetSubnetByName(string name)
        {
            var exists = await context!.Subnets!.AsExpandable().Where(p => p!.Name!.Equals(name) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<MSubnet?> GetSubnetByCidr(string cidr)
        {
            var exists = await context!.Subnets!.AsExpandable().Where(p => p!.Cidr!.Equals(cidr) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<MSubnet?> UpdateSubnetById(string subnetId, MSubnet subnet)
        {
            Guid id = Guid.Parse(subnetId);
            var existing = await context!.Subnets!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Name = subnet.Name;
                existing.Cidr = subnet.Cidr;
                existing.Tags = subnet.Tags;
            }

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<MSubnet> AddSubnet(MSubnet subnet)
        {
            subnet.OrgId = orgId;
            subnet.CreatedDate = DateTime.UtcNow;

            await context!.Subnets!.AddAsync(subnet);
            await context.SaveChangesAsync();

            return subnet;
        }

        public async Task<List<MSubnet>> GetSubnets(VMSubnet param)
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

            var predicate = SubnetPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                //r.MetaData = "";
            }

            return result;
        }

        public async Task<int> GetSubnetCount(VMSubnet param)
        {
            var predicate = SubnetPredicate(param!);
            var result = await context!.Subnets!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MSubnet?> GetSubnetById(string subnetId)
        {
            Guid id = Guid.Parse(subnetId);
            var u = await GetSelection().AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MSubnet> GetSelection()
        {
            var query =
                from sbn in context!.Subnets
                select new { sbn };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MSubnet
            {
                Id = x.sbn.Id,
                OrgId = x.sbn.OrgId,
                Cidr = x.sbn.Cidr,
                Name = x.sbn.Name,
                Tags = x.sbn.Tags,
                CreatedDate = x.sbn.CreatedDate,
            });
        }

        private ExpressionStarter<MSubnet> SubnetPredicate(VMSubnet param)
        {
            var pd = PredicateBuilder.New<MSubnet>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MSubnet>();
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Cidr!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MSubnet?> DeleteSubnetById(string subnetId)
        {
            Guid id = Guid.Parse(subnetId);
            var existing = await context!.Subnets!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.Subnets!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }
    }
}