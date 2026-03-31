using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]

    public class MVersionUpgrade
    {
        public string FromVersion { get; set; }
        public string ToVersion { get; set; }

        public MVersionUpgrade()
        {
            FromVersion = string.Empty;
            ToVersion = string.Empty;
        }
    }
}
