namespace Its.PleaseProtect.Api.AuditLogs
{
    public class AuditLog
    {
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }
        public string? Path { get; set; }
        public string? QueryString { get; set; }
        public string? UserAgent { get; set; }
        public string? Host { get; set; }
        public string? Scheme { get; set; }
        public string? ClientIp { get; set; }
        public string? CfClientIp { get; set; }
        public string? Environment { get; set; }

        public string? CustomStatus { get; set; }
        public string? CustomDesc { get; set; }

        public long? RequestSize { get; set; }
        public long? ResponseSize { get; set; }
        public long? LatencyMs { get; set; }

        public UserInfo? userInfo { get; set; }
        public object? ContextData { get; set; }

        public AuditLog()
        {
            userInfo = new UserInfo();
        }
    }
}
