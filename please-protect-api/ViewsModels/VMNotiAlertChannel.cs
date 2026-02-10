using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMNotiAlertChannel : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Status { get; set; }
    }
}
