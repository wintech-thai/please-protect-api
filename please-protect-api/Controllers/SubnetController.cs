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
    public class SubnetController : ControllerBase
    {
        private readonly ISubnetService svc;

        [ExcludeFromCodeCoverage]
        public SubnetController(ISubnetService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddSubnet")]
        public async Task<IActionResult> AddSubnet(string id, [FromBody] MSubnet request)
        {
            var result = await svc.AddSubnet(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteSubnetById/{subnetId}")]
        public async Task<IActionResult> DeleteSubnetById(string id, string subnetId)
        {
            var result = await svc.DeleteSubnetById(id, subnetId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetSubnetById/{subnetId}")]
        public async Task<IActionResult> GetSubnetById(string id, string subnetId)
        {
            var result = await svc.GetSubnetById(id, subnetId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateSubnetById/{subnetId}")]
        public async Task<IActionResult> UpdateSubnetById(string id, string subnetId, [FromBody] MSubnet request)
        {
            var result = await svc.UpdateSubnetById(id, subnetId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetSubnets")]
        public async Task<IActionResult> GetSubnets(string id, [FromBody] VMSubnet request)
        {
            var result = await svc.GetSubnets(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetSubnetCount")]
        public async Task<IActionResult> GetSubnetCount(string id, [FromBody] VMSubnet request)
        {
            var result = await svc.GetSubnetCount(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateSubnetsCache")]
        public async Task<IActionResult> UpdateSubnetsCache(string id)
        {
            var result = await svc.UpdateSubnetsCache(id);
            return Ok(result);
        }
    }
}
