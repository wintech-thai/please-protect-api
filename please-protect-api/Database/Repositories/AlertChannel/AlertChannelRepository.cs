using LinqKit;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using System.Data.Entity;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class AlertChannelRepository : BaseRepository, IAlertChannelRepository
    {
        public AlertChannelRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<MNotiAlertChannel> AddAlertChannel(MNotiAlertChannel alertChannel)
        {
            alertChannel.OrgId = orgId;
            
            alertChannel.Id = Guid.NewGuid();
            alertChannel.CreatedDate = DateTime.UtcNow;

            context!.NotiAlertChannels!.Add(alertChannel);
            await context.SaveChangesAsync();

            return alertChannel;
        }

        public async Task<List<MNotiAlertChannel>> GetAlertChannels(VMNotiAlertChannel param)
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

            var predicate = AlertChannelPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                r.DiscordWebhookUrl = "";
            }

            return result;
        }

        public async Task<int> GetAlertChannelCount(VMNotiAlertChannel param)
        {
            var predicate = AlertChannelPredicate(param!);
            var result = await context!.NotiAlertChannels!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public async Task<MNotiAlertChannel?> GetAlertChannelById(string alertChannelId)
        {
            Guid id = Guid.Parse(alertChannelId);
            var u = await context!.NotiAlertChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public IQueryable<MNotiAlertChannel> GetSelection()
        {
            var query =
                from channel in context!.NotiAlertChannels!
                select new { channel };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MNotiAlertChannel
            {
                Id = x.channel.Id,
                OrgId = x.channel.OrgId,
                ChannelName = x.channel.ChannelName,
                Description = x.channel.Description,
                Tags = x.channel.Tags,
                Type = x.channel.Type,
                Status = x.channel.Status,
                DiscordWebhookUrl = x.channel.DiscordWebhookUrl,
                CreatedDate = x.channel.CreatedDate,
            });
        }

        private ExpressionStarter<MNotiAlertChannel> AlertChannelPredicate(VMNotiAlertChannel param)
        {
            var pd = PredicateBuilder.New<MNotiAlertChannel>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MNotiAlertChannel>();
                fullTextPd = fullTextPd.Or(p => p.ChannelName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Type!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Status!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            if ((param.Status != "") && (param.Status != null))
            {
                var statusPd = PredicateBuilder.New<MNotiAlertChannel>();
                statusPd = statusPd.Or(p => p.Status!.Equals(param.Status));

                pd = pd.And(statusPd);
            }

            return pd;
        }

        public async Task<MNotiAlertChannel?> DeleteAlertChannelById(string alertChannelId)
        {
            Guid id = Guid.Parse(alertChannelId);
            var existing = await context!.NotiAlertChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.NotiAlertChannels!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MNotiAlertChannel?> UpdateAlertChannelById(string alertChannelId, MNotiAlertChannel alertChannel)
        {
            Guid id = Guid.Parse(alertChannelId);
            var existing = await context!.NotiAlertChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                // ไม่ให้ update Type
                existing.ChannelName = alertChannel.ChannelName;
                existing.Description = alertChannel.Description;
                existing.Tags = alertChannel.Tags;
                existing.DiscordWebhookUrl = alertChannel.DiscordWebhookUrl;

                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MNotiAlertChannel?> UpdateAlertChannelStatusById(string alertChannelId, string status)
        {
            Guid id = Guid.Parse(alertChannelId);
            var existing = await context!.NotiAlertChannels!.AsExpandable().Where(p => p!.Id!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.Status = status;
                await context.SaveChangesAsync();
            }
            
            return existing;
        }

        public async Task<bool> IsChannelNameExist(string channelName)
        {
            var exists = await context!.NotiAlertChannels!.AsExpandable().AnyAsync(p => p!.ChannelName!.Equals(channelName) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MNotiAlertChannel?> GetAlertChannelByName(string channelName)
        {
            var exists = await context!.NotiAlertChannels!.AsExpandable().Where(p => p!.ChannelName!.Equals(channelName) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }
    }
}