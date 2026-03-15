using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService svc;

        [ExcludeFromCodeCoverage]
        public ConfigurationController(IConfigurationService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetDomain")]
        public async Task<IActionResult> GetDomain(string id)
        {
            var result = await svc.GetDomain(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetDomain")]
        public async Task<IActionResult> SetDomain(string id, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetDomain(id, cfg.ConfigValue!);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous]
        [Route("org/{id}/action/GetLogo")]
        public async Task<IActionResult> GetLogo(string id)
        {
            var result = await svc.GetLogo(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetLogo")]
        public async Task<IActionResult> SetLogo(string id, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetLogo(id, cfg.ConfigValue!);
            return Ok(result);
        }


        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous]
        [Route("org/{id}/action/GetOrgShortName")]
        public async Task<IActionResult> GetOrgShortName(string id)
        {
            var result = await svc.GetOrgShortName(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetOrgShortName")]
        public async Task<IActionResult> SetOrgShortName(string id, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetOrgShortName(id, cfg.ConfigValue!);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous]
        [Route("org/{id}/action/GetOrgDescription")]
        public async Task<IActionResult> GetOrgDescription(string id)
        {
            var result = await svc.GetOrgDescription(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetOrgDescription")]
        public async Task<IActionResult> SetOrgDescription(string id, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetOrgDescription(id, cfg.ConfigValue!);
            return Ok(result);
        }

        // Cloud Connect related endpoints
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous]
        [Route("org/{id}/action/GetCloudUrl")]
        public async Task<IActionResult> GetCloudUrl(string id)
        {
            var result = await svc.GetCloudUrl(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetCloudUrl")]
        public async Task<IActionResult> SetCloudUrl(string id, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetCloudUrl(id, cfg.ConfigValue!);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous]
        [Route("org/{id}/action/GetCloudConnectKey")]
        public async Task<IActionResult> GetCloudConnectKey(string id)
        {
            var result = await svc.GetCloudConnectKey(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetCloudConnectKey")]
        public async Task<IActionResult> SetCloudConnectKey(string id, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetCloudConnectKey(id, cfg.ConfigValue!);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [AllowAnonymous]
        [Route("org/{id}/action/GetCloudConnectFlag")]
        public async Task<IActionResult> GetCloudConnectFlag(string id)
        {
            var result = await svc.GetCloudConnectFlag(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SetCloudConnectFlag")]
        public async Task<IActionResult> SetCloudConnectFlag(string id, [FromBody] MConfiguration cfg)
        {
            var result = await svc.SetCloudConnectFlag(id, cfg.ConfigValue!);
            return Ok(result);
        }
    }
}
