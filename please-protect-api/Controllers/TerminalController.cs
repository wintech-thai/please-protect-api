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

            var pods = await k8sClient.ListNamespacedPodAsync(
                namespaceParameter: k8sNamespace,
                labelSelector: "app=terminal");

            var pod = pods.Items.FirstOrDefault(p => p.Status.Phase == "Running");

            if (pod == null)
                throw new Exception("No running terminal pod found");

Console.WriteLine($"DEBUG1 - Pod name = [{pod.Metadata.Name}]");

            var k8sSocket = await k8sClient.WebSocketNamespacedPodExecAsync(
                pod.Metadata.Name,
                k8sNamespace,
                command: new[] { "/bin/bash", "-i" },
                container: null,
                tty: true,
                stdin: true,
                stdout: true,
                stderr: true,
                cancellationToken: CancellationToken.None);

            var buffer = new byte[8192];

Console.WriteLine("DEBUG - Starting t1");
            var t1 = Task.Run(async () =>
            {
                while (clientSocket.State == WebSocketState.Open)
                {
                    var result = await clientSocket.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                        
Console.WriteLine($"DEBUG2.1 - SEND TO POD: {Encoding.UTF8.GetString(buffer,0,result.Count)}");
                    await k8sSocket.SendAsync(
                        new ArraySegment<byte>(buffer, 0, result.Count),
                        WebSocketMessageType.Binary,
                        true,
                        CancellationToken.None);
Console.WriteLine($"DEBUG2.2 - SENT TO POD: {Encoding.UTF8.GetString(buffer,0,result.Count)}");
                }
            });

Console.WriteLine("DEBUG - Starting t2");
            var t2 = Task.Run(async () =>
            {
                while (true)
                {
Console.WriteLine("DEBUG3.1 - READING FROM POD");

                    var result = await k8sSocket.ReceiveAsync(buffer, CancellationToken.None);
Console.WriteLine($"DEBUG3.2 - COUNT: {result.Count}, TYPE: {result.MessageType}");
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    if (result.Count > 0)
                    {
                        await clientSocket.SendAsync(
                            new ArraySegment<byte>(buffer, 0, result.Count),
                            WebSocketMessageType.Binary,
                            true,
                            CancellationToken.None);
                    }
                }
            });

            await Task.WhenAll(t1, t2);
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
