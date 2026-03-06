using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Its.PleaseProtect.Api.Services
{
    public class KafkaAdminService : IKafkaAdminService
    {
        private readonly IAdminClient _admin;
        private readonly ConsumerConfig _consumerConfig;

        public KafkaAdminService(IConfiguration config)
        {
            var bootstrap = "kafka.kafka.svc.cluster.local:9092";

            _admin = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = bootstrap
            }).Build();

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrap,
                GroupId = "kafka-api-inspector",
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        public async Task<List<string>> GetTopicsAsync()
        {
            // ใช้ Task.Run เพื่อทำให้เป็น Non-blocking operation
            return await Task.Run(() =>
            {
                try
                {
                    var meta = _admin.GetMetadata(TimeSpan.FromSeconds(10));

                    return meta.Topics
                        .Where(t => !t.Topic.StartsWith("__"))
                        .Select(t => t.Topic)
                        .OrderBy(t => t) // เรียงลำดับให้ดูง่าย
                        .ToList();
                }
                catch (Exception ex)
                {
                    // พ่น Error ออกไปให้ต้นทางจัดการ
                    throw new Exception($"ไม่สามารถดึงข้อมูล Metadata ได้: {ex.Message}");
                }
            });
        }

        public async Task<object> GetTopicDetailAsync(string topic)
        {
            // 1. ครอบด้วย Task.Run เพื่อไม่ให้ block thread
            return await Task.Run(() =>
            {
                try
                {
                    // ดึง Metadata เฉพาะ Topic ที่ระบุ
                    var meta = _admin.GetMetadata(topic, TimeSpan.FromSeconds(10));
                    
                    // 2. ค้นหา Topic ที่ชื่อตรงกันจริงๆ (ป้องกัน Metadata คืนค่าว่างหรือค่าผิด)
                    var t = meta.Topics.FirstOrDefault(x => x.Topic == topic);

                    if (t == null || t.Error.IsError)
                    {
                        return (object)new { error = $"ไม่พบ Topic: {topic} หรือเกิดข้อผิดพลาด: {t?.Error.Reason}" };
                    }

                    // 3. จัด Format ข้อมูลให้สวยงาม
                    return new
                    {
                        name = t.Topic,
                        partitionCount = t.Partitions.Count,
                        partitions = t.Partitions.Select(p => new
                        {
                            partition = p.PartitionId,
                            leader = p.Leader,
                            replicas = p.Replicas,     // Broker IDs ที่เก็บข้อมูลนี้
                            isr = p.InSyncReplicas,    // Broker IDs ที่ข้อมูลอัปเดตล่าสุดพร้อมใช้งาน
                            hasError = p.Error.IsError,
                            errorReason = p.Error.Reason
                        }).OrderBy(p => p.partition).ToList() // เรียงเลข Partition ให้ดูง่าย
                    };
                }
                catch (Exception ex)
                {
                    return new { error = $"เกิดข้อผิดพลาดภายใน: {ex.Message}" };
                }
            });
        }

        public async Task<object> GetTopicOffsetsAsync(string topic)
        {
            // ครอบการทำงานทั้งหมดด้วย Task.Run เพราะ Lib เป็น Sync ทั้งแผง
            return await Task.Run(() =>
            {
                try
                {
                    // 1. ดึง Metadata เพื่อหาจำนวน Partition
                    var meta = _admin.GetMetadata(topic, TimeSpan.FromSeconds(10));
                    var topicMeta = meta.Topics.FirstOrDefault(t => t.Topic == topic);

                    if (topicMeta == null || topicMeta.Error.IsError)
                    {
                        return (object)new { error = $"ไม่พบ Topic: {topic}" };
                    }

                    // 2. ใช้ Consumer แค่ตัวเดียวในการ Query ทุก Partition (ดีกว่าสร้างใน Loop)
                    using var consumer = new ConsumerBuilder<Ignore, Ignore>(_consumerConfig).Build();
                    
                    var result = new List<object>();

                    foreach (var p in topicMeta.Partitions)
                    {
                        var tp = new TopicPartition(topic, p.PartitionId);
                        
                        // Query หา Low (Earliest) และ High (Latest) Watermarks
                        var watermark = consumer.QueryWatermarkOffsets(tp, TimeSpan.FromSeconds(10));

                        result.Add(new
                        {
                            partition = p.PartitionId,
                            earliestOffset = watermark.Low.Value,
                            latestOffset = watermark.High.Value,
                            messageCount = watermark.High.Value - watermark.Low.Value // จำนวน Message ที่มีอยู่ใน Partition นั้นจริง ๆ
                        });
                    }

                    return new
                    {
                        topic,
                        totalPartitions = result.Count,
                        partitions = result
                    };
                }
                catch (Exception ex)
                {
                    return new { error = $"เกิดข้อผิดพลาด: {ex.Message}" };
                }
            });
        }

        public async Task<List<string>> GetConsumerGroupsAsync()
        {
            // ครอบด้วย Task.Run เพื่อไม่ให้ Block Thread หลักของแอป
            return await Task.Run(() =>
            {
                try
                {
                    // ดึงข้อมูล Metadata ของ Groups ทั้งหมดใน Cluster
                    var groupsMetadata = _admin.ListGroups(TimeSpan.FromSeconds(10));

                    // ตรวจสอบว่ามีข้อมูลกลับมาหรือไม่
                    if (groupsMetadata == null)
                    {
                        return new List<string>();
                    }

                    return groupsMetadata
                        .Select(g => g.Group)
                        .OrderBy(g => g) // เรียงชื่อ Group เพื่อให้หาได้ง่ายบน UI/Logs
                        .ToList();
                }
                catch (Exception ex)
                {
                    // ในงานจริงอาจจะ Logging ไว้ แล้วคืนค่า List ว่าง หรือ Throw ต่อก็ได้ครับ
                    throw new Exception($"ไม่สามารถดึงรายชื่อ Consumer Groups ได้: {ex.Message}");
                }
            });
        }

        public async Task<object> GetConsumerLagAsync(string groupId)
        {
            try
            {
                // 1. ดึง Committed Offsets ทั้งหมดของ Group นี้ (ทุก Topic/Partition ที่เคยกิน)
                var groupOffsetsList = await _admin.ListConsumerGroupOffsetsAsync(new[] {
                    new ConsumerGroupTopicPartitions(groupId, null) 
                });

                var groupReport = groupOffsetsList.FirstOrDefault();
                if (groupReport == null || groupReport.Partitions == null || !groupReport.Partitions.Any())
                {
                    return new { Message = $"No offsets found for group: {groupId}" };
                }

                // 2. สร้าง Consumer ชั่วคราวเพื่อหา High Watermark
                using var tempConsumer = new ConsumerBuilder<Ignore, Ignore>(_consumerConfig).Build();

                var results = new List<object>();
                long totalLag = 0;

                // 3. วนลูปตาม Partition ที่ Group นี้มี Committed Offset อยู่
                foreach (var info in groupReport.Partitions)
                {
                    // ดึง High Watermark ของ Topic/Partition นั้นๆ
                    var watermark = tempConsumer.QueryWatermarkOffsets(info.TopicPartition, TimeSpan.FromSeconds(10));
                    
                    long high = watermark.High;
                    long committed = info.Offset.IsSpecial ? 0 : info.Offset.Value;
                    
                    long lag = high - committed;
                    if (lag < 0) lag = 0;
                    totalLag += lag;

                    results.Add(new
                    {
                        Topic = info.Topic,
                        Partition = info.Partition.Value,
                        HighWatermark = high,
                        CommittedOffset = committed,
                        Lag = lag
                    });
                }

                // 4. สรุปผลแยกตาม Topic ให้ด้วยเพื่อให้ดูง่ายขึ้น
                var summaryByTopic = results.Cast<dynamic>()
                    .GroupBy(r => (string)r.Topic)
                    .Select(g => new {
                        Topic = g.Key,
                        TopicLag = g.Sum(x => (long)x.Lag),
                        Partitions = g.ToList()
                    });

                return new
                {
                    GroupId = groupId,
                    TotalLagAcrossAllTopics = totalLag,
                    Topics = summaryByTopic
                };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        public async Task<List<string>> GetConsumerGroupsForTopicAsync(string topic)
        {
            var result = new List<string>();

            try
            {
                // 1. ดึงรายชื่อ Groups ทั้งหมดที่มีอยู่ใน Cluster
                // หมายเหตุ: ListGroups เป็น Synchronous ใน Library นี้ (ส่งกลับ Metadata)
                var groupsMetadata = _admin.ListGroups(TimeSpan.FromSeconds(10));

                // 2. เตรียม Parameter เพื่อไปถามว่าแต่ละ Group เกาะ Topic ไหนบ้าง
                // เราจะส่งรายชื่อทุก Group ไปถามทีเดียวเพื่อ Performance ที่ดีกว่า
                var options = groupsMetadata.Select(g => new ConsumerGroupTopicPartitions(g.Group, null)).ToList();

                // 3. ใช้ Method ที่ถูกต้องสำหรับ v2.x คือ ListConsumerGroupOffsetsAsync
                var allGroupsOffsets = await _admin.ListConsumerGroupOffsetsAsync(options);

                // 4. กรองดูว่า Group ไหนที่มี Offset บันทึกไว้ใน Topic ที่เราสนใจ
                foreach (var groupReport in allGroupsOffsets)
                {
                    // ตรวจสอบว่าในบรรดา Partition ที่ Group นี้ถืออยู่ มี Topic ที่เราหาหรือไม่
                    bool isConsumingTopic = groupReport.Partitions.Any(p => p.Topic == topic);

                    if (isConsumingTopic)
                    {
                        result.Add(groupReport.Group);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error เช่น KafkaException
                Console.WriteLine($"Error fetching groups: {ex.Message}");
            }

            return result;
        }
        
        public async Task<object> GetTopicLagAsync(string topic)
        {
            // 1. ดึง Metadata เพื่อหาว่า Topic นี้มี Partition อะไรบ้าง
            var metadata = _admin.GetMetadata(topic, TimeSpan.FromSeconds(10));
            var topicMetadata = metadata.Topics.FirstOrDefault(t => t.Topic == topic);
            if (topicMetadata == null) return new List<object>();

            // 2. ดึง High Watermarks ของทุก Partition ไว้ก่อน (ดึงครั้งเดียวใช้ได้กับทุก Group)
            using var consumer = new ConsumerBuilder<Ignore, Ignore>(_consumerConfig).Build();
            var watermarks = new Dictionary<int, long>();
            foreach (var p in topicMetadata.Partitions)
            {
                var hw = consumer.QueryWatermarkOffsets(new TopicPartition(topic, p.PartitionId), TimeSpan.FromSeconds(10));
                watermarks[p.PartitionId] = hw.High;
            }

            // 3. ดึงรายชื่อ Groups ทั้งหมด
            var groupsMetadata = _admin.ListGroups(TimeSpan.FromSeconds(10));
            
            // 4. เตรียม Request ดึง Offset ของทุก Group พร้อมกัน
            var options = groupsMetadata.Select(g => new ConsumerGroupTopicPartitions(g.Group, null)).ToList();
            var allGroupsOffsets = await _admin.ListConsumerGroupOffsetsAsync(options);

            var result = new List<object>();

            // 5. จับคู่ข้อมูล (Matching)
            foreach (var groupReport in allGroupsOffsets)
            {
                // กรองเอาเฉพาะ Partition ที่เป็นของ Topic ที่เราสนใจ
                var relevantPartitions = groupReport.Partitions.Where(p => p.Topic == topic);

                foreach (var p in relevantPartitions)
                {
                    long high = watermarks.ContainsKey(p.Partition.Value) ? watermarks[p.Partition.Value] : 0;
                    long committed = p.Offset.IsSpecial ? 0 : p.Offset.Value;

                    result.Add(new
                    {
                        group = groupReport.Group,
                        topic,
                        partition = p.Partition.Value,
                        committedOffset = committed,
                        latestOffset = high,
                        lag = high - committed
                    });
                }
            }

            return result;
        }
    }
}