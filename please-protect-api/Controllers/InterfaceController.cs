using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
                var token = await System.IO.File.ReadAllTextAsync("/var/run/secrets/kubernetes.io/serviceaccount/token");

                _kubeClient.DefaultRequestHeaders.Clear();
                _kubeClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var workloads = new List<KubeWorkload>
                {
                    new("censor-arkime", "daemonsets", "daemonset"),
                    new("censor-zeek", "deployments", "zeek-eth0"),
                    new("censor-suricata", "statefulsets", "suricata-eth0")
                };
Log.Information($"DEBUG A....");
                foreach (var w in workloads)
                {
Log.Information($"DEBUG B....");
                    // 1. get workload
                    var workloadUrl = $"/apis/apps/v1/namespaces/{w.Namespace}/{w.Type}/{w.Name}";
                    var workloadResp = await _kubeClient.GetAsync(workloadUrl);

                    if (!workloadResp.IsSuccessStatusCode)
                        throw new Exception($"Get workload failed: {w.Namespace}/{w.Name}");
Log.Information($"DEBUG C....");
                    var workloadJson = await workloadResp.Content.ReadAsStringAsync();
                    using var workloadDoc = JsonDocument.Parse(workloadJson);

                    var selector = workloadDoc.RootElement
                        .GetProperty("spec")
                        .GetProperty("selector")
                        .GetProperty("matchLabels");

                    // 2. build labelSelector string
                    var labelList = new List<string>();
                    foreach (var prop in selector.EnumerateObject())
                    {
                        labelList.Add($"{prop.Name}={prop.Value.GetString()}");
                    }

                    var labelSelector = string.Join(",", labelList);
Log.Information($"DEBUG D selector [{labelSelector}]");

                    // 3. list pods
                    var listUrl = $"/api/v1/namespaces/{w.Namespace}/pods?labelSelector={Uri.EscapeDataString(labelSelector)}";
                    var listResp = await _kubeClient.GetAsync(listUrl);

                    if (!listResp.IsSuccessStatusCode)
                        throw new Exception($"List pods failed: {w.Namespace}");

                    var podJson = await listResp.Content.ReadAsStringAsync();
                    using var podDoc = JsonDocument.Parse(podJson);

                    var items = podDoc.RootElement.GetProperty("items");
Log.Information($"DEBUG E item count [{items.EnumerateArray().Count()}]");
                    // 4. delete pods
                    foreach (var pod in items.EnumerateArray())
                    {
                        var podName = pod.GetProperty("metadata").GetProperty("name").GetString();
                        Log.Information($"DELETING pod [{podName}] from namespace [{w.Namespace}]...");

                        var deleteUrl = $"/api/v1/namespaces/{w.Namespace}/pods/{podName}";
                        var deleteResp = await _kubeClient.DeleteAsync(deleteUrl);

                        if (!deleteResp.IsSuccessStatusCode)
                        {
                            var err = await deleteResp.Content.ReadAsStringAsync();
                            throw new Exception($"Delete failed: {podName} -> {err}");
                        }
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
