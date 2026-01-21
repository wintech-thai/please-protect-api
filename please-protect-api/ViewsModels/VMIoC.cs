using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMIoC : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? DataSet { get; set; }
        public string? IocType { get; set; }
    }
}
