using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Its.PleaseProtect.Api.Utils;
using Serilog;


namespace Its.PleaseProtect.Api.AuditLogs
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;
        private readonly IRedisHelper _redis;

        public AuditLogMiddleware(RequestDelegate next,
            IHttpClientFactory httpClientFactory,
            IRedisHelper redis)
        {
            _next = next;
            _httpClient = httpClientFactory.CreateClient();
            _redis = redis;
        }

        private string? GetValue(HttpContext context, string key, string defaultValue)
        {
            var value = context.Items[key];
            if (value == null)
            {
                return defaultValue;
            }

            return value.ToString();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var scheme = context.Request.Scheme;
            var method = context.Request.Method;
            var host = context.Request.Headers["X-Forwarded-Host"].ToString();
            var path = context.Request.Path;
            var query = context.Request.QueryString.ToString();
            var fullUrl = $"{method} {path}{query}";
            var requestSize = context.Request.ContentLength ?? 0;
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            var cfClientIp = "";
            if (context.Request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                cfClientIp = context.Request.Headers["CF-Connecting-IP"].ToString();
            }

            var clientIp = "";
            if (context.Request.Headers.TryGetValue("X-Original-Forwarded-For", out var xForwardedFor))
            {
                clientIp = xForwardedFor.ToString().Split(',')[0].Trim();
            }

            await _next(context); // call next middleware

            var custStatus = "";
            if (context.Response.Headers.TryGetValue("CUST_STATUS", out var customStatus))
            {
                custStatus = customStatus;
            }

            var responseSize = memoryStream.Length;
            var statusCode = context.Response.StatusCode;
  
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            var statusDesc = "";
            if (statusCode != 200)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
                statusDesc = responseBody;
            }
            else
            {
                if (context.Response.Headers.TryGetValue("CUST_DESC", out var statusDescHeader))
                {
                    statusDesc = statusDescHeader;
                }
            }

            context.Items.TryGetValue("ContextData", out var contextData);

            stopwatch.Stop();

            var latencyMs = stopwatch.ElapsedMilliseconds;

            // === Build log JSON ===
            var logObject = new AuditLog()
            {
                Host = host,
                HttpMethod = method,
                StatusCode = statusCode,
                Path = path,
                QueryString = query,
                UserAgent = userAgent,
                RequestSize = requestSize,
                ResponseSize = responseSize,
                LatencyMs = latencyMs,
                Scheme = scheme,
                ClientIp = clientIp,
                CfClientIp = cfClientIp,
                CustomStatus = custStatus,
                CustomDesc = statusDesc,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),

                userInfo = new UserInfo()
                {
                    Role = GetValue(context, "Temp-Authorized-Role", ""),
                    CustomRole = GetValue(context, "Temp-Authorized-CustomRole", ""),
                    IdentityType = GetValue(context, "Temp-Identity-Type", ""),
                    UserId = GetValue(context, "Temp-Identity-Id", ""),
                    UserName = GetValue(context, "Temp-Identity-Name", ""),
                },

                ContextData = contextData,
            };

            if (path == "/health")
            {
                //No need to audit log for health check
                return;
            }

            var logJson = JsonSerializer.Serialize(logObject);
            Log.Information(logJson);

            PublishMessage(logObject);
        }

        private void PublishMessage(AuditLog auditLog)
        {
            var endpoint = Environment.GetEnvironmentVariable("LOG_ENDPOINT");

            if (string.IsNullOrWhiteSpace(endpoint))
                return;

            _ = Task.Run(async () =>
            {
                try
                {
                    var json = JsonSerializer.Serialize(auditLog);

                    using var content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );

                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = content
                    };

                    request.Headers.Add("User-Agent", "PleaseProtect-AuditLog");

                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Warning("AuditLog publish failed. Status: {StatusCode}", response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "AuditLog publish exception");
                }
            });
        }
    }
}
