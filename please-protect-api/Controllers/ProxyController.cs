using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
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
        private readonly HttpClient _arkimeClient;

        private static readonly Regex[] AllowedKubeApiRegex =
        {
            // -----------------------
            // namespaces
            // -----------------------
            new(@"^api/v1/namespaces/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // -----------------------
            // pods & events (all namespaces)
            // -----------------------
            new(@"^api/v1/pods/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            new(@"^api/v1/events/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // -----------------------
            // pods & events (namespaced)
            // -----------------------
            new(@"^api/v1/namespaces/[^/]+/(pods|events)(/[^/]+)?/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // -----------------------
            // workloads (apps/v1)
            // all namespaces
            // -----------------------
            new(@"^apis/apps/v1/(deployments|statefulsets|daemonsets|replicasets)(/[^/]+)?/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // namespaced workloads
            new(@"^apis/apps/v1/namespaces/[^/]+/(deployments|statefulsets|daemonsets|replicasets)(/[^/]+)?/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // -----------------------
            // metrics
            // -----------------------
            new(@"^apis/metrics\.k8s\.io/v1beta1/pods/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            new(@"^apis/metrics\.k8s\.io/v1beta1/namespaces/[^/]+/pods/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),
        };

        [ExcludeFromCodeCoverage]
        public ProxyController(IHttpClientFactory factory)
        {
            _esClient = factory.CreateClient("es-proxy");
            _promClient = factory.CreateClient("prom-proxy");
            _lokiClient = factory.CreateClient("loki-proxy");
            _kubeClient = factory.CreateClient("kube-proxy");
            _arkimeClient = factory.CreateClient("arkime-proxy");
        }

        [ExcludeFromCodeCoverage]
        [AcceptVerbs("GET", "POST")]
        [Route("org/{id}/action/Arkime/{**path}")]
        public async Task Arkime(string id, string path, CancellationToken ct)
        {
            // -------------------------
            // Security: Arkime allow list
            // -------------------------
            var allowed = new[]
            {
                "api/sessions",
                "api/session",
                "api/spiview",
                "api/connections",
                "api/files",
                "api/fields",
                "api/stats"
            };

            if (!allowed.Any(a =>
                path.StartsWith(a, StringComparison.OrdinalIgnoreCase)))
            {
//Console.WriteLine($"DEBUG1 - API not allowed [{path}]");
                Response.StatusCode = StatusCodes.Status403Forbidden;
                await Response.WriteAsync("API not allowed");
                return;
            }
//Console.WriteLine($"DEBUG2 - API allowed [{path}]");
            // -------------------------
            // build target request
            // -------------------------
            var targetUri = $"/{path}{Request.QueryString}";

            using var requestMessage = new HttpRequestMessage(
                new HttpMethod(Request.Method),
                targetUri
            );

            // copy body (streaming)
            if (Request.ContentLength > 0)
            {
                requestMessage.Content = new StreamContent(Request.Body);
            }

            // -------------------------
            // copy headers (safe)
            // -------------------------
            foreach (var header in Request.Headers)
            {
                // ไม่ copy header ที่ระบบจัดการเอง
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!requestMessage.Headers.TryAddWithoutValidation(
                        header.Key, header.Value.ToArray()))
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(
                        header.Key, header.Value.ToArray());
                }
            }

            // Arkime required header
            requestMessage.Headers.TryAddWithoutValidation(
                "X-Requested-With", "XMLHttpRequest");

            // -------------------------
            // send to Arkime
            // -------------------------
            using var responseMessage = await _arkimeClient.SendAsync(
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

            Response.ContentType =
                responseMessage.Content.Headers.ContentType?.ToString()
                ?? "application/json";

            // -------------------------
            // stream body back
            // -------------------------
            await responseMessage.Content.CopyToAsync(Response.Body);
        }


        private static bool IsAllowedKubePath(string path)
        {
            path = path.Trim('/');

            // block dangerous pod subresources
            if (Regex.IsMatch(path,
                @"pods/.+/(exec|attach|portforward|proxy|log)$",
                RegexOptions.IgnoreCase))
            {
                return false;
            }

            return AllowedKubeApiRegex.Any(r => r.IsMatch(path));
        }

        [AcceptVerbs("GET")]
        [Route("org/{id}/action/Kube/{**path}")]
        public async Task Kube(string id, string path, CancellationToken ct)
        {
            path ??= "";

            if (!IsAllowedKubePath(path))
            {
//Console.WriteLine($"DEBUG1 - API not allowed [{path}]");
                Response.StatusCode = 403;
                await Response.WriteAsync("API not allowed");
                return;
            }

//Console.WriteLine($"DEBUG2 - API allowed [{path}]");
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
