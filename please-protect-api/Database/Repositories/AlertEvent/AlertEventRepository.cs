using LinqKit;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using System.Data.Entity;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class AlertEventRepository : BaseRepository, IAlertEventRepository
    {
        public AlertEventRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MNotiAlertEvent> AddAlertEvent(MNotiAlertEvent alertEvent)
        {
            alertEvent.OrgId = orgId;
            
            alertEvent.Id = Guid.NewGuid();
            alertEvent.CreatedDate = DateTime.UtcNow;

            context!.NotiAlertEvents!.Add(alertEvent);
            await context.SaveChangesAsync();

            return alertEvent;
        }

        public async Task<List<MNotiAlertEvent>> GetAlertEvents(VMNotiAlertEvent param)
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

            var predicate = AlertEventPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                r.RawData = "";
            }

            return result;
        }

        public async Task<int> GetAlertEventCount(VMNotiAlertEvent param)
        {
            var predicate = AlertEventPredicate(param!);
            var result = await context!.NotiAlertEvents!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MNotiAlertEvent?> GetAlertEventById(string alertEventId)
        {
            Guid id = Guid.Parse(alertEventId);
            var u = await context!.NotiAlertEvents!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MNotiAlertEvent> GetSelection()
        {
            var query =
                from alert in context!.NotiAlertEvents
                select new { alert };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MNotiAlertEvent
            {
                Id = x.alert.Id,
                OrgId = x.alert.OrgId,
                Name = x.alert.Name,
                Summary = x.alert.Summary,
                Detail = x.alert.Detail,
                RawData = x.alert.RawData,
                Severity = x.alert.Severity,
                Status = x.alert.Status,
                CreatedDate = x.alert.CreatedDate,
            });
        }

        private ExpressionStarter<MNotiAlertEvent> AlertEventPredicate(VMNotiAlertEvent param)
        {
            var pd = PredicateBuilder.New<MNotiAlertEvent>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MNotiAlertEvent>();
                fullTextPd = fullTextPd.Or(p => p.Name!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Summary!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Detail!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Severity!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Status!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.Severity != "") && (param.Severity != null))
            {
                var severityPd = PredicateBuilder.New<MNotiAlertEvent>();
                severityPd = severityPd.Or(p => p.Severity!.Equals(param.Severity));

                pd = pd.And(severityPd);
            }

            return pd;
        }
    }
}