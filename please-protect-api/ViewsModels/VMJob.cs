using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMJob : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? JobType { get; set; }
        public string? DocumentId { get; set; }
    }
}
