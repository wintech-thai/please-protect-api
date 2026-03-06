
namespace Its.PleaseProtect.Api.Services
{
    public interface IKafkaAdminService
    {
        Task<List<string>> GetTopicsAsync();
        Task<object> GetTopicDetailAsync(string topic);
        Task<object> GetTopicOffsetsAsync(string topic);

        Task<List<string>> GetConsumerGroupsAsync();
        Task<object> GetConsumerLagAsync(string groupId);

        Task<List<string>> GetConsumerGroupsForTopicAsync(string topic);
        Task<object> GetTopicLagAsync(string topic);
    }
}
