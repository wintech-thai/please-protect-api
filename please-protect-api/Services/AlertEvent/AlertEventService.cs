using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Utils;
using System.Text.Json;
using Serilog;

namespace Its.PleaseProtect.Api.Services
{
    public class AlertEventService : BaseService, IAlertEventService
    {
        private readonly IAlertEventRepository? repository = null;
        private readonly IAlertChannelService _alertChannelSvc;

        public AlertEventService(IAlertEventRepository repo, IAlertChannelService alertChannelService) : base()
        {
            repository = repo;
            _alertChannelSvc = alertChannelService;
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
            var status = firstAlert.Status != null ? firstAlert.Status : "firing";

            var evt = new MNotiAlertEvent()
            {
                Name = alertName,
                Summary = summary,
                Detail = detail,
                RawData = JsonSerializer.Serialize(alertEvent),
                Severity = severity,
                Status = status,
            };

            var result = await repository.AddAlertEvent(evt);

            var vm = new VMNotiAlertChannel()
            {
                Status = "Enabled"
            }; 
            
            var channels = await _alertChannelSvc.GetAlertChannels(orgId, vm);
            foreach (var channel in channels)
            {
                if (channel.Status != "Enabled")
                {
                    Log.Information($"Skipping channel: [{channel.ChannelName}], status is [{channel.Status}]");
                    continue;
                }

                var channelType = channel.Type!.ToLower();
                if (channelType == "discord")
                {
                    var url = channel.DiscordWebhookUrl!;
                    var discordNotifier = new DiscordNotifier(new HttpClient(), url);

                    try
                    {
                        await discordNotifier.SendAsync(alertEvent!);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error sending Discord alert. Channel: [{channel.ChannelName}], Url: [{url}]");
                        Log.Error(ex.Message);
                        // ไม่ต้อง throw → loop จะไปต่อ
                    }
                }
            }

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
