using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService svc;

        [ExcludeFromCodeCoverage]
        public ApplicationController(IApplicationService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetApplications")]
        public IActionResult GetApplications(string id)
        {
            var result = svc.GetApplications(id);
            return Ok(result);
        }
    }
}
