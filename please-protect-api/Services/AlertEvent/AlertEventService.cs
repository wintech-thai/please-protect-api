using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Utils;
using System.Text.Json;
using FluentValidation;

namespace Its.PleaseProtect.Api.Services
{
    public class AlertEventService : BaseService, IAlertEventService
    {
        private readonly IAlertEventRepository? repository = null;

        public AlertEventService(IAlertEventRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVNotiAlertEvent> Notify(string orgId, AlertmanagerWebhook alertEvent)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiAlertEvent()
            {
                Status = "OK",
                Description = "Success"
            };

            var firstAlert = alertEvent?.Alerts?.FirstOrDefault();
            if (firstAlert == null)
            {
                r.Status = "NO_ALERT";
                r.Description = "No alert found in the request";

                return r;
            }

            var alertName = firstAlert.Labels != null && firstAlert.Labels.ContainsKey("alertname") ? firstAlert.Labels["alertname"] : "Unnamed Alert";
            var summary = firstAlert.Annotations != null && firstAlert.Annotations.ContainsKey("summary") ? firstAlert.Annotations["summary"] : "No Summary";
            var detail = firstAlert.Annotations != null && firstAlert.Annotations.ContainsKey("description") ? firstAlert.Annotations["description"] : "No Detail"; 
            var severity = firstAlert.Labels != null && firstAlert.Labels.ContainsKey("severity") ? firstAlert.Labels["severity"] : "unknown";

            var evt = new MNotiAlertEvent()
            {
                Name = alertName,
                Summary = summary,
                Detail = detail,
                RawData = JsonSerializer.Serialize(alertEvent),
                Severity = severity,
            };

            var result = await repository.AddAlertEvent(evt);

            r.NotiAlertEvent = result;
            return r;
        }

        public async Task<MVNotiAlertEvent> GetAlertEventById(string orgId, string alertEventId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiAlertEvent()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(alertEventId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Alert Event ID [{alertEventId}] format is invalid";

                return r;
            }

            var result = await repository!.GetAlertEventById(alertEventId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Alert Event ID [{alertEventId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiAlertEvent = result;

            return r;
        }

        public async Task<List<MNotiAlertEvent>> GetAlertEvents(string orgId, VMNotiAlertEvent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAlertEvents(param);

            return result;
        }

        public async Task<int> GetAlertEventCount(string orgId, VMNotiAlertEvent param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAlertEventCount(param);

            return result;
        }
    }
}
