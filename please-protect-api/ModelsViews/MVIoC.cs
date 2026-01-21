using System.Diagnostics.CodeAnalysis;
using Its.PleaseProtect.Api.Models;

namespace Its.PleaseProtect.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVIoC
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MIoC? IoC { get; set; }
    }
}
