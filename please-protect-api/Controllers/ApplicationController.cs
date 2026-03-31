using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.Utils;
using Its.PleaseProtect.Api.Models;

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

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetCurrentAppCustomConfig/{appName}")]
        public async Task<IActionResult> GetCurrentAppCustomConfig(string id, string appName)
        {
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            var result = await svc.GetCurrentAppCustomConfig(id, git, appName);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetDraftAppCustomConfig/{appName}")]
        public async Task<IActionResult> GetDraftAppCustomConfig(string id, string appName)
        {
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            var result = await svc.GetDraftAppCustomConfig(id, git, appName);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/SaveDraftAppCustomConfig/{appName}")]
        public async Task<IActionResult> SaveDraftAppCustomConfig(string id, string appName, [FromBody] string content)
        {
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            var result = await svc.SaveDraftAppCustomConfig(id, git, appName, content);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/MergeDraftAppCustomConfig/{appName}")]
        public async Task<IActionResult> MergeDraftAppCustomConfig(string id, string appName)
        {
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            var result = await svc.MergeDraftAppCustomConfig(id, git, appName);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/VersionUpgrade")]
        public async Task<IActionResult> VersionUpgrade(string id, [FromBody] MVersionUpgrade versionUpgrade)
        {
            var result = await svc.UpgradeVersion(id, versionUpgrade);
            return Ok(result);
        }
    }
}
