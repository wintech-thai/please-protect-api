using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
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
        private readonly HttpClient _kubeClient;
        
        // helper record (สั้น กระชับ)
        private record KubeWorkload(string Namespace, string Type, string Name);

        [ExcludeFromCodeCoverage]
        public InterfaceController(IHttpClientFactory factory)
        {
            // ดึง client ที่ชื่อ "if-manager" จาก factory (ต้องตั้งค่าใน Program.cs ก่อน)
            _ifManagerClient = factory.CreateClient("if-manager");
            _kubeClient = factory.CreateClient("kube-proxy");
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetInterfaces")]
        public async Task<IActionResult> GetInterfaces(string id)
        {
            // GET /interfaces
            var response = await _ifManagerClient.GetAsync("interfaces/all");
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

        private async Task<bool> RestartSensors()
        {
            try
            {
                // 1. อ่าน service account token
                var token = await System.IO.File.ReadAllTextAsync("/var/run/secrets/kubernetes.io/serviceaccount/token");

                // 2. set header (ใช้ client ที่มี BaseAddress อยู่แล้ว)
                _kubeClient.DefaultRequestHeaders.Clear();
                _kubeClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 3. define workloads (extend ได้ง่าย)
                var workloads = new List<KubeWorkload>
                {
                    new("censor-arkime", "daemonsets", "daemonset"),
                    new("censor-zeek", "deployments", "zeek-eth0"),
                    new("censor-suricata", "statefulsets", "suricata-eth0")
                };

                // 4. restart ทีละ workload
                foreach (var w in workloads)
                {
                    var url = $"/apis/apps/v1/namespaces/{w.Namespace}/{w.Type}/{w.Name}";

                    var patchObj = new
                    {
                        spec = new
                        {
                            template = new
                            {
                                metadata = new
                                {
                                    annotations = new Dictionary<string, string>
                                    {
                                        ["kubectl.kubernetes.io/restartedAt"] = DateTime.UtcNow.ToString("o")
                                    }
                                }
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(patchObj);
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/strategic-merge-patch+json")
                    };

                    var response = await _kubeClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Restart failed: {w.Namespace}/{w.Type}/{w.Name} -> {err}");
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/EnableInterface/{interfaceId}")]
        public async Task<IActionResult> EnableInterface(string id, string interfaceId)
        {
            // POST /interfaces/{interfaceId}/enable
            // ใช้ content เป็น null หากไม่ต้องส่ง body
            var response = await _ifManagerClient.PostAsync($"interfaces/{interfaceId}/enable", null);

            if (response.IsSuccessStatusCode)
            {
                _ = RestartSensors();
            }

            return await HandleResponse(response);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DisableInterface/{interfaceId}")]
        public async Task<IActionResult> DisableInterface(string id, string interfaceId)
        {
            // POST /interfaces/{interfaceId}/disable
            var response = await _ifManagerClient.PostAsync($"interfaces/{interfaceId}/disable", null);
            
            if (response.IsSuccessStatusCode)
            {
                _ = RestartSensors();
            }

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
