using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using k8s;
using System.Text;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class TerminalController : ControllerBase
    {
        [ExcludeFromCodeCoverage]
        public TerminalController()
        {
        }
        private async Task HandleTerminal(WebSocket clientSocket)
        {
            var k8sNamespace = "terminal";
            Console.WriteLine($"[DEBUG] 1. Starting HandleTerminal in namespace: {k8sNamespace}");

            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                var k8sClient = new Kubernetes(config);

                // --- จุดเช็ค 1: ค้นหา Pod ---
                var pods = await k8sClient.ListNamespacedPodAsync(
                    namespaceParameter: k8sNamespace,
                    labelSelector: "app=terminal");

                Console.WriteLine($"[DEBUG] 2. Found {pods.Items.Count} pods with label app=terminal");

                var pod = pods.Items.FirstOrDefault(p => p.Status.Phase == "Running");

                if (pod == null)
                {
                    Console.WriteLine("[DEBUG] ERROR: No running pod found!");
                    return;
                }
                Console.WriteLine($"[DEBUG] 3. Target Pod Name: {pod.Metadata.Name}");

                // --- จุดเช็ค 2: การสร้าง Connection กับ K8s (จุดนี้มักจะค้างถ้า RBAC ไม่พอ) ---
                Console.WriteLine("[DEBUG] 4. Attempting to open K8s WebSocket Stream...");
                using var k8sStream = await k8sClient.WebSocketNamespacedPodExecAsync(
                    pod.Metadata.Name,
                    k8sNamespace,
                    command: new[] { "/bin/bash", "-i" },
                    container: null,
                    tty: true,
                    stdin: true,
                    stdout: true,
                    stderr: true,
                    cancellationToken: CancellationToken.None);
                
                Console.WriteLine("[DEBUG] 5. K8s Stream Opened Successfully.");

                using var cts = new CancellationTokenSource();
                var clientToK8s = Task.Run(async () =>
                {
                    var buffer = new byte[8192];
                    while (clientSocket.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                    {
                        var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        if (result.MessageType == WebSocketMessageType.Close) break;

                        // สร้าง Array ใหม่ที่ใหญ่ขึ้น 1 Byte เพื่อใส่ Channel 0 ไว้ข้างหน้า
                        var k8sBuffer = new byte[result.Count + 1];
                        k8sBuffer[0] = 0; // Channel 0 คือ stdin
                        Array.Copy(buffer, 0, k8sBuffer, 1, result.Count);

                        Console.WriteLine($"[DEBUG] >> SEND TO K8S (with Channel 0): {Encoding.UTF8.GetString(buffer, 0, result.Count)}");

                        await k8sStream.SendAsync(
                            new ArraySegment<byte>(k8sBuffer),
                            WebSocketMessageType.Binary,
                            true,
                            cts.Token);
                    }
                });

                // Task 2: Kubernetes -> Client (ตัด Byte แรกทิ้งเพื่อเอา Data จริง)
                var k8sToClient = Task.Run(async () =>
                {
                    var buffer = new byte[8192];
                    while (k8sStream.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                    {
                        var result = await k8sStream.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        if (result.MessageType == WebSocketMessageType.Close) break;

                        if (result.Count > 1) // ต้องมากกว่า 1 เพราะ Byte แรกคือ Channel
                        {
                            var channel = buffer[0];
                            var dataLength = result.Count - 1;
                            
                            // Channel 1 คือ stdout, 2 คือ stderr
                            if (channel == 1 || channel == 2) 
                            {
                                var output = Encoding.UTF8.GetString(buffer, 1, dataLength);
                                Console.WriteLine($"[DEBUG] << FROM POD (Channel {channel}): {output}");

                                await clientSocket.SendAsync(
                                    new ArraySegment<byte>(buffer, 1, dataLength),
                                    WebSocketMessageType.Binary,
                                    true,
                                    cts.Token);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] FATAL ERROR: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"[DEBUG] Inner: {ex.InnerException.Message}");
            }
            finally
            {
                //cts.Cancel();
                Console.WriteLine("[DEBUG] Session Terminated.");
            }
        }
        
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/Connect")]
        public async Task Connect(string id)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            using var clientSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleTerminal(clientSocket);
        }
    }
}
