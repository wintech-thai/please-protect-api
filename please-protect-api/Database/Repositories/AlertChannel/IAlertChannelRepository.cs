using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IAlertChannelRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<List<MNotiAlertChannel>> GetAlertChannels(VMNotiAlertChannel param);
        public Task<int> GetAlertChannelCount(VMNotiAlertChannel param);
        public Task<MNotiAlertChannel?> GetAlertChannelById(string alertChannelId);
        public Task<MNotiAlertChannel> AddAlertChannel(MNotiAlertChannel alertChannel);
        public Task<MNotiAlertChannel?> DeleteAlertChannelById(string alertChannelId);
        public Task<MNotiAlertChannel?> UpdateAlertChannelById(string alertChannelId, MNotiAlertChannel alertChannel);
        public Task<MNotiAlertChannel?> UpdateAlertChannelStatusById(string alertChannelId, string status);
        public Task<MNotiAlertChannel?> GetAlertChannelByName(string channelName);
        public Task<bool> IsChannelNameExist(string channelName);
    }
}
