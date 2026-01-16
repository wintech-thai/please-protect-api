using System.Diagnostics.CodeAnalysis;
using Its.PleaseProtect.Api.Services;

namespace Its.PleaseProtect.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPresignedUrl
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public PresignedPostResult? PresignedResult { get; set; }
    }
}
