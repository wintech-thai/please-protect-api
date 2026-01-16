using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Services;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService svc;

        [ExcludeFromCodeCoverage]
        public ApiKeyController(IApiKeyService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        //Check if API Key found & not expire in the specific organization
        [Route("org/{id}/action/VerifyApiKey/{apiKey}")]
        public IActionResult VerifyApiKey(string id, string apiKey)
        {
            var result = svc.VerifyApiKey(id, apiKey);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddApiKey")]
        public IActionResult AddApiKey(string id, [FromBody] MApiKey request)
        {
            var result = svc.AddApiKey(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteApiKeyById/{keyId}")]
        public IActionResult DeleteApiKeyById(string id, string keyId)
        {
            var result = svc.DeleteApiKeyById(id, keyId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        // Use POST method, in the future we might send the body
        [HttpPost]
        [Route("org/{id}/action/GetApiKeys")]
        public IActionResult GetApiKeys(string id, [FromBody] VMApiKey param)
        {
            var result = svc.GetApiKeys(id, param);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetApiKeyCount")]
        public IActionResult GetApiKeyCount(string id, [FromBody] VMApiKey param)
        {
            var result = svc.GetApiKeyCount(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateApiKeyById/{keyId}")]
        public IActionResult UpdateApiKeyById(string id, string keyId, [FromBody] MApiKey request)
        {
            var result = svc.UpdateApiKeyById(id, keyId, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableApiKeyById/{keyId}")]
        public IActionResult EnableApiKeyById(string id, string keyId)
        {
            var result = svc.UpdateApiKeyStatusById(id, keyId, "Active");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableApiKeyById/{keyId}")]
        public IActionResult DisableApiKeyById(string id, string keyId)
        {
            var result = svc.UpdateApiKeyStatusById(id, keyId, "Disabled");
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetApiKeyById/{keyId}")]
        public async Task<MVApiKey> GetApiKeyById(string id, string keyId)
        {
            var result = await svc.GetApiKeyById(id, keyId);
            return result;
        }
    }
}
