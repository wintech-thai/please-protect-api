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
            using var cts = new CancellationTokenSource();
            
            Console.WriteLine($"[DEBUG] 1. Starting HandleTerminal in namespace: {k8sNamespace}");

            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                var k8sClient = new Kubernetes(config);

                // --- ค้นหา Pod ---
                var pods = await k8sClient.ListNamespacedPodAsync(
                    namespaceParameter: k8sNamespace,
                    labelSelector: "app=terminal");

                var pod = pods.Items.FirstOrDefault(p => p.Status.Phase == "Running");
                if (pod == null)
                {
                    Console.WriteLine("[DEBUG] ERROR: No running pod found!");
                    return;
                }
                Console.WriteLine($"[DEBUG] 2. Target Pod Name: {pod.Metadata.Name}");

                // --- เปิด K8s WebSocket Stream ---
                // ใช้ /bin/sh เพื่อความครอบคลุมสูงสุด
                using var k8sStream = await k8sClient.WebSocketNamespacedPodExecAsync(
                    pod.Metadata.Name,
                    k8sNamespace,
                    command: new[] { "/bin/sh", "-i" }, 
                    container: null,
                    tty: true,
                    stdin: true,
                    stdout: true,
                    stderr: true,
                    cancellationToken: CancellationToken.None);
                
                Console.WriteLine("[DEBUG] 3. K8s Stream Opened Successfully.");

                // --- ส่ง Terminal Resize (Channel 4) เพื่อ "ปลุก" Shell ---
                var resizeMsg = "{\"Width\":120,\"Height\":40}";
                var resizeBuffer = new byte[resizeMsg.Length + 1];
                resizeBuffer[0] = 4; // Channel 4: Resize
                Encoding.UTF8.GetBytes(resizeMsg, 0, resizeMsg.Length, resizeBuffer, 1);
                await k8sStream.SendAsync(new ArraySegment<byte>(resizeBuffer), WebSocketMessageType.Binary, true, CancellationToken.None);
                Console.WriteLine("[DEBUG] 4. Sent Terminal Resize signal.");

                // --- Task 1: Client -> Pod (Stdin) ---
                var clientToK8s = Task.Run(async () =>
                {
                    var buffer = new byte[8192];
                    try {
                        while (clientSocket.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                        {
                            var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close) break;

                            if (result.Count > 0)
                            {
                                // ใส่ Channel 0 (stdin) นำหน้าข้อมูล
                                var k8sIn = new byte[result.Count + 1];
                                k8sIn[0] = 0; 
                                Array.Copy(buffer, 0, k8sIn, 1, result.Count);

                                await k8sStream.SendAsync(new ArraySegment<byte>(k8sIn), WebSocketMessageType.Binary, true, cts.Token);
                                //Console.WriteLine($"[DEBUG] >> SENT {result.Count} bytes to Pod");
                            }
                        }
                    } catch (Exception ex) { Console.WriteLine($"[DEBUG] ClientToK8s Error: {ex.Message}"); }
                    finally { Console.WriteLine("[DEBUG] ClientToK8s Task Ended"); }
                });

                // --- Task 2: Pod -> Client (Stdout/Stderr) ---
                var k8sToClient = Task.Run(async () =>
                {
                    var buffer = new byte[8192];
                    try {
                        while (k8sStream.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                        {
                            var result = await k8sStream.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close) break;

                            if (result.Count > 1)
                            {
                                byte channel = buffer[0];
                                // รับเฉพาะ Channel 1 (stdout) หรือ 2 (stderr)
                                if (channel == 1 || channel == 2)
                                {
                                    // ตัด Byte แรกออกก่อนส่งให้ Client
                                    await clientSocket.SendAsync(
                                        new ArraySegment<byte>(buffer, 1, result.Count - 1),
                                        WebSocketMessageType.Text, // เปลี่ยนเป็น Text เพื่อให้แสดงผลบน Browser ง่ายขึ้น
                                        true,
                                        cts.Token);
                                    
                                    var output = Encoding.UTF8.GetString(buffer, 1, result.Count - 1);
                                    //Console.WriteLine($"[DEBUG] << RECV {result.Count-1} bytes from Pod (Ch:{channel})");
                                }
                            }
                        }
                    } catch (Exception ex) { Console.WriteLine($"[DEBUG] K8sToClient Error: {ex.Message}"); }
                    finally { Console.WriteLine("[DEBUG] K8sToClient Task Ended"); }
                });

                // รอจนกว่าตัวใดตัวหนึ่งจะจบ
                var completedTask = await Task.WhenAny(clientToK8s, k8sToClient);
                Console.WriteLine($"[DEBUG] 5. Session loop broken by: {(completedTask == clientToK8s ? "Client Side" : "Pod Side")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] FATAL: {ex.Message}");
            }
            finally
            {
                cts.Cancel();
                if (clientSocket.State == WebSocketState.Open)
                {
                    await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
                Console.WriteLine("[DEBUG] 6. HandleTerminal Finished.");
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
