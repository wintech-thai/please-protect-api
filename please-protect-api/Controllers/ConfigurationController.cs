using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;

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
        [Route("org/{id}/action/GetEsConfig")]
        public IActionResult GetEsConfig(string id)
        {
            var result = svc.GetEsConfig(id);
            return Ok(result);
        }
    }
}
