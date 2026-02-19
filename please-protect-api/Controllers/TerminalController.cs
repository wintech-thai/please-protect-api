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
            var config = KubernetesClientConfiguration.InClusterConfig();
            var k8sClient = new Kubernetes(config);

            // 1. ค้นหา Pod ที่ต้องการเชื่อมต่อ
            var pods = await k8sClient.ListNamespacedPodAsync(
                namespaceParameter: k8sNamespace,
                labelSelector: "app=terminal");

            var pod = pods.Items.FirstOrDefault(p => p.Status.Phase == "Running");

            if (pod == null)
            {
                await clientSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "No running terminal pod found", CancellationToken.None);
                return;
            }

            // 2. สร้างการเชื่อมต่อกับ Kubernetes Exec (ใช้ using เพื่อจัดการ disposal)
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

            // 3. ใช้ CancellationToken เพื่อประสานการปิด Task ทั้งสองฝั่ง
            using var cts = new CancellationTokenSource();

            try
            {
                // Task 1: Client -> Kubernetes (User พิมพ์คำสั่ง)
                var clientToK8s = Task.Run(async () =>
                {
                    var buffer = new byte[8192]; // แยก Buffer เฉพาะของตัวเอง
                    while (clientSocket.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                    {
                        var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        
                        if (result.MessageType == WebSocketMessageType.Close) break;

                        // ส่ง Data ไปยัง Pod (K8s Stream มักจะคาดหวัง Binary)
                        await k8sStream.SendAsync(
                            new ArraySegment<byte>(buffer, 0, result.Count),
                            WebSocketMessageType.Binary,
                            true,
                            cts.Token);
                    }
                }, cts.Token);

                // Task 2: Kubernetes -> Client (แสดงผลลัพธ์จาก Terminal)
                var k8sToClient = Task.Run(async () =>
                {
                    var buffer = new byte[8192]; // แยก Buffer เฉพาะของตัวเอง
                    while (k8sStream.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                    {
                        var result = await k8sStream.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        
                        if (result.MessageType == WebSocketMessageType.Close) break;

                        if (result.Count > 0)
                        {
                            await clientSocket.SendAsync(
                                new ArraySegment<byte>(buffer, 0, result.Count),
                                WebSocketMessageType.Binary,
                                true,
                                cts.Token);
                        }
                    }
                }, cts.Token);

                // รอจนกว่า Task ใด Task หนึ่งจะจบลง หรือเกิด Error
                await Task.WhenAny(clientToK8s, k8sToClient);
            }
            catch (OperationCanceledException) { /* การยกเลิกปกติ */ }
            catch (Exception ex)
            {
                Console.WriteLine($"Terminal Error: {ex.Message}");
            }
            finally
            {
                // ส่งสัญญาณยกเลิก Task ที่เหลืออยู่ และปิดการเชื่อมต่อทั้งหมด
                cts.Cancel();
                
                if (clientSocket.State != WebSocketState.Aborted)
                {
                    await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Terminal session ended", CancellationToken.None);
                }
                Console.WriteLine("Terminal Session Closed.");
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
