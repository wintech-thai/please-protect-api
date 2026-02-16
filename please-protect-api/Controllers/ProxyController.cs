using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _esClient;
        private readonly HttpClient _promClient;
        private readonly HttpClient _lokiClient;
        private readonly HttpClient _kubeClient;

        [ExcludeFromCodeCoverage]
        public ProxyController(IHttpClientFactory factory)
        {
            _esClient = factory.CreateClient("es-proxy");
            _promClient = factory.CreateClient("prom-proxy");
            _lokiClient = factory.CreateClient("loki-proxy");
            _kubeClient = factory.CreateClient("kube-proxy");
        }

        private static bool HasDangerousSubresource(string[] segments)
        {
            if (segments.Length < 7)
                return false;

            var dangerous = new[]
            {
                "exec",
                "attach",
                "portforward",
                "proxy",
                "log"
            };

            return dangerous.Contains(
                segments.Last(),
                StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsAllowedKubePath(string path)
        {
            path = path.Trim('/');

            var segments = path.Split('/');

            // minimum check
            if (segments.Length < 6)
                return false;

            // ------------------------
            // core api (/api/v1/...)
            // ------------------------
            if (segments[0] == "api" &&
                segments[1] == "v1" &&
                segments[2] == "namespaces")
            {
                var resource = segments[4];

                return resource switch
                {
                    "pods" => !HasDangerousSubresource(segments),
                    "events" => true,
                    _ => false
                };
            }

            // ------------------------
            // apps api
            // ------------------------
            if (segments[0] == "apis" &&
                segments[1] == "apps")
            {
                var resource = segments[6];

                return resource switch
                {
                    "deployments" => true,
                    "statefulsets" => true,
                    "daemonsets" => true,
                    "replicasets" => true,
                    _ => false
                };
            }

            // ------------------------
            // metrics api
            // ------------------------
            if (segments[0] == "apis" &&
                segments[1] == "metrics.k8s.io")
            {
                return segments[6] == "pods";
            }

            return false;
        }

        [AcceptVerbs("GET")]
        [Route("org/{id}/action/Kube/{**path}")]
        public async Task Kubernetes(string id, string path, CancellationToken ct)
        {
            path ??= "";

            if (!IsAllowedKubePath(path))
            {
                Response.StatusCode = 403;
                await Response.WriteAsync("API not allowed");
                return;
            }

            var targetUri = $"{path}{Request.QueryString}";

            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, targetUri);

            foreach (var header in Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;

                requestMessage.Headers.TryAddWithoutValidation(
                    header.Key, header.Value.ToArray());
            }

            // -------------------------
            // ADD K8S TOKEN
            // -------------------------
            var token = await System.IO.File.ReadAllTextAsync(
                "/var/run/secrets/kubernetes.io/serviceaccount/token",
                ct);

            requestMessage.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    token.Trim()
                );

            using var responseMessage = await _kubeClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            Response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var h in responseMessage.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            foreach (var h in responseMessage.Content.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            Response.Headers.Remove("transfer-encoding");

            await responseMessage.Content.CopyToAsync(Response.Body);
        }

        [AcceptVerbs("GET","POST")]
        [Route("org/{id}/action/Loki/{**path}")]
        public async Task Loki(string id, string path, CancellationToken ct)
        {
            var blockedPrefixes = new[]
            {
                "api/v1/push",      // ingest logs
                "api/prom/push",
                "api/v1/delete"
            };

            if (blockedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                Response.StatusCode = 403;
                await Response.WriteAsync("Write API not allowed");
                return;
            }

            var targetUri = $"{path}{Request.QueryString}";

            using var requestMessage = new HttpRequestMessage(
                new HttpMethod(Request.Method),
                targetUri
            );

            // copy headers (block Authorization)
            foreach (var header in Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;

                requestMessage.Headers.TryAddWithoutValidation(
                    header.Key, header.Value.ToArray());
            }

            using var responseMessage = await _lokiClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            Response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var h in responseMessage.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            foreach (var h in responseMessage.Content.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            Response.Headers.Remove("transfer-encoding");

            await responseMessage.Content.CopyToAsync(Response.Body);
        }

        [AcceptVerbs("GET","POST")]
        [Route("org/{id}/action/Prometheus/{**path}")]
        public async Task Prometheus(string id, string path, CancellationToken ct)
        {
            // -------------------------
            // Allow เฉพาะ API ที่ปลอดภัย
            // -------------------------
            var allowedPrefixes = new[]
            {
                "api/v1/query",
                "api/v1/query_range",
                "api/v1/series",
                "api/v1/labels",
                "api/v1/label"
            };

            if (!allowedPrefixes.Any(p =>
                path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                Response.StatusCode = 403;
                await Response.WriteAsync("API not allowed");
                return;
            }

            var targetUri = $"{path}{Request.QueryString}";

            using var requestMessage = new HttpRequestMessage(
                new HttpMethod(Request.Method),
                targetUri
            );

            // copy headers (block Authorization)
            foreach (var header in Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;

                requestMessage.Headers.TryAddWithoutValidation(
                    header.Key, header.Value.ToArray());
            }

            using var responseMessage = await _promClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            Response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var h in responseMessage.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            foreach (var h in responseMessage.Content.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            Response.Headers.Remove("transfer-encoding");

            await responseMessage.Content.CopyToAsync(Response.Body);
        }
        
        [ExcludeFromCodeCoverage]
        [AcceptVerbs("GET","POST","PUT","DELETE","PATCH","HEAD")]
        [Route("org/{id}/action/ElasticSearch/{**path}")]
        public async Task ElasticSearch(string id, string path, CancellationToken ct)
        {
            // -------------------------
            // Security: allow list only
            // -------------------------
            var allowed = new[]
            {
                "_search",
                "_bulk",
                "_doc",
                "_count",
                "_msearch"
            };

            if (!allowed.Any(a => path.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                await Response.WriteAsync("API not allowed");
                return;
            }

            // -------------------------
            // build target request
            // -------------------------
            var targetUri = $"{path}{Request.QueryString}";

            using var requestMessage = new HttpRequestMessage(
                new HttpMethod(Request.Method),
                targetUri
            );

            // copy body (streaming)
            if (Request.ContentLength > 0)
            {
                requestMessage.Content = new StreamContent(Request.Body);
            }

            // copy headers
            foreach (var header in Request.Headers)
            {
                // skip headers ที่ ASP.NET จัดการเอง
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // -------------------------
            // send to ES (stream mode)
            // -------------------------
            using var responseMessage = await _esClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                ct
            );

            // -------------------------
            // copy status code
            // -------------------------
            Response.StatusCode = (int)responseMessage.StatusCode;

            // -------------------------
            // copy headers back
            // -------------------------
            foreach (var header in responseMessage.Headers)
                Response.Headers[header.Key] = header.Value.ToArray();

            foreach (var header in responseMessage.Content.Headers)
                Response.Headers[header.Key] = header.Value.ToArray();

            Response.Headers.Remove("transfer-encoding");

            Response.ContentType = responseMessage.Content.Headers.ContentType?.ToString() ?? "application/json";

            // -------------------------
            // stream body back
            // -------------------------
            await responseMessage.Content.CopyToAsync(Response.Body);
        }
    }
}
