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
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService svc;

        [ExcludeFromCodeCoverage]
        public DocumentController(IDocumentService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddDocument")]
        public async Task<IActionResult> AddDocument(string id, [FromBody] MDocument request)
        {
            var result = await svc.AddDocument(id, request);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteDocumentById/{documentId}")]
        public async Task<IActionResult> DeleteDocumentById(string id, string documentId)
        {
            var result = await svc.DeleteDocumentById(id, documentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetDocumentById/{documentId}")]
        public async Task<IActionResult> GetDocumentById(string id, string documentId)
        {
            var result = await svc.GetDocumentById(id, documentId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateDocumentById/{documentId}")]
        public async Task<IActionResult> UpdateDocumentById(string id, string documentId, [FromBody] MDocument request)
        {
            var result = await svc.UpdateDocumentById(id, documentId, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetDocuments")]
        public async Task<IActionResult> GetDocuments(string id, [FromBody] VMDocument request)
        {
            var result = await svc.GetDocuments(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetDocumentCount")]
        public async Task<IActionResult> GetDocumentCount(string id, [FromBody] VMDocument request)
        {
            var result = await svc.GetDocumentCount(id, request);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/GetUploadPresignedUrl")]
        public async Task<IActionResult> GetUploadPresignedUrl(string id, [FromBody] VMPresignedRequest request)
        {
            var result = await svc.GetDocumentPostUploadUrl(id, request);
            return Ok(result);
        }
    }
}
