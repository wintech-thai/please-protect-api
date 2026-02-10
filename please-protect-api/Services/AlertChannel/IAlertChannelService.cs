using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Services
{
    public interface IAlertChannelService
    {
        public Task<MVNotiAlertChannel> GetAlertChannelById(string orgId, string alertChannelId);
        public Task<MVNotiAlertChannel> AddAlertChannel(string orgId, MNotiAlertChannel alertChannel);
        public Task<MVNotiAlertChannel> DeleteAlertChannelById(string orgId, string alertChannelId);
        public Task<List<MNotiAlertChannel>> GetAlertChannels(string orgId, VMNotiAlertChannel param);
        public Task<int> GetAlertChannelCount(string orgId, VMNotiAlertChannel param);
        public Task<MVNotiAlertChannel> UpdateAlertChannelById(string orgId, string alertChannelId, MNotiAlertChannel alertChannel);
        public Task<MVNotiAlertChannel> UpdateAlertChannelStatusById(string orgId, string alertChannelId, string status);
    }
}
