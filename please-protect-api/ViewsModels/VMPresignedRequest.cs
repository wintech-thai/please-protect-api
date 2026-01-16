using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPresignedRequest : VMQueryBase
    {
        public string? FileName { get; set; }
        public string? DocumentType { get; set; }
    }
}
