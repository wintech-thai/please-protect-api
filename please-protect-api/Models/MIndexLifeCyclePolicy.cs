using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MIndexLifeCyclePolicy
    {
        public string PolicyName { get; set; }
        public long LinkedIndices { get; set; }
        public long WarmDayCount { get; set; }
        public long ColdDayCount { get; set; }
        public long DeleteDayCount { get; set; }

        public MIndexLifeCyclePolicy()
        {
            PolicyName = "";
            LinkedIndices = 0;
            WarmDayCount = 7;
            ColdDayCount = 15;
            DeleteDayCount = 30;
        }
    }
}
