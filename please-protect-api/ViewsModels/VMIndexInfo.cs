using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMIndexInfo : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
