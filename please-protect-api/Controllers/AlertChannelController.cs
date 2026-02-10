using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class AlertChannelController : ControllerBase
    {
        private readonly IAlertChannelService svc;

        [ExcludeFromCodeCoverage]
        public AlertChannelController(IAlertChannelService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddAlertChannel")]
        public async Task<IActionResult> AddAlertChannel(string id, [FromBody] MNotiAlertChannel request)
        {
            var result = await svc.AddAlertChannel(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteAlertChannelById/{alertChannelId}")]
        public async Task<IActionResult> DeleteAlertChannelById(string id, string alertChannelId)
        {
            var result = await svc.DeleteAlertChannelById(id, alertChannelId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAlertChannelById/{alertChannelId}")]
        public async Task<IActionResult> GetAlertChannelById(string id, string alertChannelId)
        {
            var result = await svc.GetAlertChannelById(id, alertChannelId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateAlertChannelById/{alertChannelId}")]
        public async Task<IActionResult> UpdateAlertChannelById(string id, string alertChannelId, [FromBody] MNotiAlertChannel request)
        {
            var result = await svc.UpdateAlertChannelById(id, alertChannelId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAlertChannels")]
        public async Task<IActionResult> GetAlertChannels(string id, [FromBody] VMNotiAlertChannel request)
        {
            var result = await svc.GetAlertChannels(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAlertChannelCount")]
        public async Task<IActionResult> GetAlertChannelCount(string id, [FromBody] VMNotiAlertChannel request)
        {
            var result = await svc.GetAlertChannelCount(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableAlertChannelById/{alertChannelId}")]
        public async Task<IActionResult> EnableAlertChannelById(string id, string alertChannelId)
        {
            var result = await svc.UpdateAlertChannelStatusById(id, alertChannelId, "Enabled");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableAlertChannelById/{alertChannelId}")]
        public async Task<IActionResult> DisableAlertChannelById(string id, string alertChannelId)
        {
            var result = await svc.UpdateAlertChannelStatusById(id, alertChannelId, "Disabled");
            return Ok(result);
        }
    }
}
