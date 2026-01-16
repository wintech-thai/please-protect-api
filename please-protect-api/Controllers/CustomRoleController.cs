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
    public class CustomRoleController : ControllerBase
    {
        private readonly ICustomRoleService svc;

        [ExcludeFromCodeCoverage]
        public CustomRoleController(ICustomRoleService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInitialUserRolePermissions")]
        public IActionResult GetInitialUserRolePermissions(string id)
        {
            var result = svc.GetInitialUserRolePermissions(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddCustomRole")]
        public async Task<IActionResult> AddCustomRole(string id, [FromBody] MCustomRole request)
        {
            var result = await svc.AddCustomRole(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteCustomRoleById/{customRoleId}")]
        public async Task<IActionResult> DeleteCustomRoleById(string id, string customRoleId)
        {
            var result = await svc.DeleteCustomRoleById(id, customRoleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetCustomRoleById/{customRoleId}")]
        public async Task<IActionResult> GetCustomRoleById(string id, string customRoleId)
        {
            var result = await svc.GetCustomRoleById(id, customRoleId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateCustomRoleById/{customRoleId}")]
        public async Task<IActionResult> UpdateCustomRoleById(string id, string customRoleId, [FromBody] MCustomRole request)
        {
            var result = await svc.UpdateCustomRoleById(id, customRoleId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetCustomRoles")]
        public async Task<IActionResult> GetCustomRoles(string id, [FromBody] VMCustomRole request)
        {
            var result = await svc.GetCustomRoles(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetCustomRoleCount")]
        public async Task<IActionResult> GetCustomRoleCount(string id, [FromBody] VMCustomRole request)
        {
            var result = await svc.GetCustomRoleCount(id, request);
            return Ok(result);
        }
    }
}
