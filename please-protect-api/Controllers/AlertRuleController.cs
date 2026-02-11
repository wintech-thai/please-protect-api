using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class AlertRuleController : ControllerBase
    {
        private readonly HttpClient _promClient;

        [ExcludeFromCodeCoverage]
        public AlertRuleController(IHttpClientFactory factory)
        {
            _promClient = factory.CreateClient("prom-proxy");
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAlertRules")]
        public async Task GetAlertRules(string id, CancellationToken ct)
        {
            var targetUri = $"api/v1/rules{Request.QueryString}";

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
    }
}
