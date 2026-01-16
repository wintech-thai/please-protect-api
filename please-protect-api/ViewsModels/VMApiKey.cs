using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMApiKey : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
