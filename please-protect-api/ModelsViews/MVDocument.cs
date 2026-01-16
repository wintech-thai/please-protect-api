using System.Diagnostics.CodeAnalysis;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVDocument
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MDocument? Document { get; set; }
    }
}
