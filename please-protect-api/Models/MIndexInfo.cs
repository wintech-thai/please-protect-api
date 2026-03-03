using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MIndexInfo
    {
        public string IndexName { get; set; }
        public string Health { get; set; }
        public string Status { get; set; }
        public long DocCount { get; set; }
        public long StoreSizeBytes { get; set; }
        public string StoreSizeHuman { get; set; }
        public string IlmPhase { get; set; }
        public DateTime? CreationDate { get; set; }
        public int PrimaryShards { get; set; }
        public int Replicas { get; set; }
        public string Codec { get; set; } = "default";
        public string CompressionAlgorithm { get; set; } = "LZ4";
        public double? EstimatedAvgDocSizeBytes { get; init; }

        public MIndexInfo()
        {
            IndexName = string.Empty;
            Health = string.Empty;
            Status = string.Empty;
            DocCount = 0;
            StoreSizeBytes = 0;
            StoreSizeHuman = string.Empty;
            IlmPhase = string.Empty;
            CreationDate = null;
            PrimaryShards = 0;
            Replicas = 0;

        }
    }
}
