using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService svc;
        private readonly string gitSyncBaseDir = "/tmp/git";

        [ExcludeFromCodeCoverage]
        public ApplicationController(IApplicationService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetApplications")]
        public async Task<IActionResult> GetApplications(string id)
        {
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            var result = await svc.GetApplications(id, git, true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetCurrentAppDefaultConfig/{appName}")]
        public async Task<IActionResult> GetCurrentAppDefaultConfig(string id, string appName)
        {
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            var result = await svc.GetCurrentAppDefaultConfig(id, git, appName);
            return Ok(result);
        }
    }
}
