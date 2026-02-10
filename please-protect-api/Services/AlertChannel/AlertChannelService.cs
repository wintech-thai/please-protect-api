using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Utils;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Text.Json;

namespace Its.PleaseProtect.Api.Services
{
    public class AlertChannelService : BaseService, IAlertChannelService
    {
        private readonly IAlertChannelRepository? repository = null;


        public AlertChannelService(IAlertChannelRepository repo) : base()
        {
            repository = repo;
        }

        public async Task<MVNotiAlertChannel> GetAlertChannelById(string orgId, string alertChannelId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiAlertChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(alertChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Alert channel ID [{alertChannelId}] format is invalid";

                return r;
            }   

            var result = await repository!.GetAlertChannelById(alertChannelId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Alert channel ID [{alertChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiAlertChannel = result;
            return r;
        }

        public async Task<MVNotiAlertChannel> AddAlertChannel(string orgId, MNotiAlertChannel alertChannel)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiAlertChannel();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(alertChannel.ChannelName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Alert channel name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsChannelNameExist(alertChannel.ChannelName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Alert channel name [{alertChannel.ChannelName}] already exist!!!";

                return r;
            }

            var result = await repository!.AddAlertChannel(alertChannel);
            if (result == null)
            {
                r.Status = "FAILED";
                r.Description = $"Failed to add alert channel [{alertChannel.ChannelName}]";

                return r;
            }

            r.NotiAlertChannel = result;

            return r;
        }

        public async Task<MVNotiAlertChannel> DeleteAlertChannelById(string orgId, string alertChannelId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiAlertChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(alertChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Alert channel ID [{alertChannelId}] format is invalid";

                return r;
            }

            var currentCr = await GetAlertChannelById(orgId, alertChannelId);
            if (currentCr.NotiAlertChannel == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Alert channel ID [{alertChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            var m = await repository!.DeleteAlertChannelById(alertChannelId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Alert channel ID [{alertChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiAlertChannel = m;
            return r;
        }

        public async Task<List<MNotiAlertChannel>> GetAlertChannels(string orgId, VMNotiAlertChannel param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAlertChannels(param);

            return result;
        }

        public async Task<int> GetAlertChannelCount(string orgId, VMNotiAlertChannel param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetAlertChannelCount(param);

            return result;
        }

        public async Task<MVNotiAlertChannel> UpdateAlertChannelById(string orgId, string alertChannelId, MNotiAlertChannel alertChannel)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiAlertChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(alertChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Alert channel ID [{alertChannelId}] format is invalid";

                return r;
            }

            var channelName = alertChannel.ChannelName;
            if (string.IsNullOrEmpty(channelName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Alert channel name is missing!!!";

                return r;
            }
            
            var m = await repository!.GetAlertChannelByName(channelName);
            if (m != null)
            {
                if (m.Id.ToString() != alertChannelId) 
                {
                    r.Status = "NAME_DUPLICATE";
                    r.Description = $"Alert channel name [{channelName}] already exist!!!";

                    return r;
                }
            }

            var result = await repository!.UpdateAlertChannelById(alertChannelId, alertChannel);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Alert channel ID [{alertChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiAlertChannel = result;
            return r;
        }

        public async Task<MVNotiAlertChannel> UpdateAlertChannelStatusById(string orgId, string alertChannelId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVNotiAlertChannel()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(alertChannelId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Alert channel ID [{alertChannelId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateAlertChannelStatusById(alertChannelId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Alert channel ID [{alertChannelId}] not found for the organization [{orgId}]";

                return r;
            }

            r.NotiAlertChannel = result;
            return r;
        }
    }
}
