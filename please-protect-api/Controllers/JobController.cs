using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService svc;

        public JobController(IJobService service)
        {
            svc = service;
        }

        [HttpGet]
        [Route("org/{id}/action/GetJobById/{jobId}")]
        public IActionResult GetJobById(string id, string jobId)
        {
            var result = svc.GetJobById(id, jobId);
            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteJobById/{jobId}")]
        public IActionResult DeleteJobById(string id, string jobId)
        {
            var result = svc.DeleteJobById(id, jobId);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetJobs")]
        public IActionResult GetJobs(string id, [FromBody] VMJob param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetJobs(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetJobCount")]
        public IActionResult GetJobCount(string id, [FromBody] VMJob param)
        {
            var result = svc.GetJobCount(id, param);
            return Ok(result);
        }
    }
}
