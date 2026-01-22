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
    public class IoCController : ControllerBase
    {
        private readonly IIoCService svc;

        [ExcludeFromCodeCoverage]
        public IoCController(IIoCService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteIoCById/{iocId}")]
        public async Task<IActionResult> DeleteIoCById(string id, string iocId)
        {
            var result = await svc.DeleteIoCById(id, iocId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetIoCById/{iocId}")]
        public async Task<IActionResult> GetIoCById(string id, string iocId)
        {
            var result = await svc.GetIoCById(id, iocId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetIoCs")]
        public async Task<IActionResult> GetIoCs(string id, [FromBody] VMIoC request)
        {
            var result = await svc.GetIoCs(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetIoCCount")]
        public async Task<IActionResult> GetIoCCount(string id, [FromBody] VMIoC request)
        {
            var result = await svc.GetIoCCount(id, request);
            return Ok(result);
        }
    }
}
