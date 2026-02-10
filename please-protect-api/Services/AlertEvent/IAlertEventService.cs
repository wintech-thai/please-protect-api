using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Services
{
    public interface IAlertEventService
    {
        public Task<MVNotiAlertEvent> Notify(string orgId, AlertmanagerWebhook alertEvent);
        public Task<MVNotiAlertEvent> GetAlertEventById(string orgId, string alertEventId);
        public Task<List<MNotiAlertEvent>> GetAlertEvents(string orgId, VMNotiAlertEvent param);
        public Task<int> GetAlertEventCount(string orgId, VMNotiAlertEvent param);
    }
}
