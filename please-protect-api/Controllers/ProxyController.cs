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

        [ExcludeFromCodeCoverage]
        public ProxyController(IHttpClientFactory factory)
        {
            _esClient = factory.CreateClient("es-proxy");
            _promClient = factory.CreateClient("prom-proxy");
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
