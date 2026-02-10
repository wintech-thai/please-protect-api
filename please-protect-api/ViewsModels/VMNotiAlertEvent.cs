using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMNotiAlertEvent : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Severity { get; set; }
    }
}
