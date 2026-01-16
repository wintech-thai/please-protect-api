using System.Diagnostics.CodeAnalysis;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVApiKey
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MApiKey? ApiKey { get; set; }
    }
}
