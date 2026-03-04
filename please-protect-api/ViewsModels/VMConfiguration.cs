using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMConfiguration : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? ConfigType { get; set; }
    }
}
