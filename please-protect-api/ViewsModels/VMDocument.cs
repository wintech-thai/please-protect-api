using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMDocument : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
