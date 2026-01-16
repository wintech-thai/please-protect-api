namespace Its.PleaseProtect.Api.Services
{
    public class PresignedPostResult
    {
        public string Url { get; init; } = default!;

        public IDictionary<string, string> Fields { get; init; }
            = new Dictionary<string, string>();

        public string? ObjectKey { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public string Provider { get; init; } = default!;
    }
}
