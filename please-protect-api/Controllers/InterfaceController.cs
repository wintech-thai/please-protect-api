using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class InterfaceController : ControllerBase
    {
        private readonly HttpClient _ifManagerClient;

        [ExcludeFromCodeCoverage]
        public InterfaceController(IHttpClientFactory factory)
        {
            // ดึง client ที่ชื่อ "if-manager" จาก factory (ต้องตั้งค่าใน Program.cs ก่อน)
            _ifManagerClient = factory.CreateClient("if-manager");
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInterfaces")]
        public async Task<IActionResult> GetInterfaces(string id)
        {
            // GET /interfaces
            var response = await _ifManagerClient.GetAsync("interfaces");
            return await HandleResponse(response);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInterfacesActivity")]
        public async Task<IActionResult> GetInterfacesActivity(string id)
        {
            // GET /interfaces/activity
            var response = await _ifManagerClient.GetAsync("interfaces/activity");
            return await HandleResponse(response);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableInterface/{interfaceId}")]
        public async Task<IActionResult> EnableInterface(string id, string interfaceId)
        {
            // POST /interfaces/{interfaceId}/enable
            // ใช้ content เป็น null หากไม่ต้องส่ง body
            var response = await _ifManagerClient.PostAsync($"interfaces/{interfaceId}/enable", null);
            return await HandleResponse(response);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableInterface/{interfaceId}")]
        public async Task<IActionResult> DisableInterface(string id, string interfaceId)
        {
            // POST /interfaces/{interfaceId}/disable
            var response = await _ifManagerClient.PostAsync($"interfaces/{interfaceId}/disable", null);
            return await HandleResponse(response);
        }

        /// <summary>
        /// Helper สำหรับอ่าน Content จาก if-manager และส่งกลับไปในรูปแบบ JSON
        /// </summary>
        private async Task<IActionResult> HandleResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // ส่งต่อ Status Code และ Content กลับไปให้ Client ของเรา
            return StatusCode((int)response.StatusCode, content);
        }
    }
}
