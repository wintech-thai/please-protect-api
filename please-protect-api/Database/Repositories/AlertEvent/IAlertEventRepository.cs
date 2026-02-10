using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IAlertEventRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<List<MNotiAlertEvent>> GetAlertEvents(VMNotiAlertEvent param);
        public Task<int> GetAlertEventCount(VMNotiAlertEvent param);
        public Task<MNotiAlertEvent?> GetAlertEventById(string alertEventId);
        public Task<MNotiAlertEvent> AddAlertEvent(MNotiAlertEvent alertEvent);
    }
}
