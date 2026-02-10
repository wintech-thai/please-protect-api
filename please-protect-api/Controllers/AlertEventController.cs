using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class AlertEventController : ControllerBase
    {
        private readonly IAlertEventService svc;

        [ExcludeFromCodeCoverage]
        public AlertEventController(IAlertEventService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [AllowAnonymous]
        [Route("org/{id}/action/Notify")]
        public async Task<IActionResult> Notify(string id, [FromBody] AlertmanagerWebhook request)
        {
            var result = await svc.Notify(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAlertEventById/{alertEventId}")]
        public async Task<IActionResult> GetAlertEventById(string id, string alertEventId)
        {
            var result = await svc.GetAlertEventById(id, alertEventId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAlertEvents")]
        public async Task<IActionResult> GetAlertEvents(string id, [FromBody] VMNotiAlertEvent request)
        {
            var result = await svc.GetAlertEvents(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetAlertEventCount")]
        public async Task<IActionResult> GetAlertEventCount(string id, [FromBody] VMNotiAlertEvent request)
        {
            var result = await svc.GetAlertEventCount(id, request);
            return Ok(result);
        }
    }
}
