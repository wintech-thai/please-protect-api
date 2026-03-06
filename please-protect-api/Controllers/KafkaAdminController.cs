using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.PleaseProtect.Api.Services;

namespace Its.PleaseProtect.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class KafkaAdminController : ControllerBase
    {
        private readonly IKafkaAdminService _kafka;

        [ExcludeFromCodeCoverage]
        public KafkaAdminController(IKafkaAdminService kafka)
        {
            _kafka = kafka;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetTopics")]
        public async Task<IActionResult> GetTopics(string id)
        {
            var result = await _kafka.GetTopicsAsync();
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetTopicByName/{topic}")]
        public async Task<IActionResult> GetTopicByName(string id, string topic)
        {
            var result = await _kafka.GetTopicDetailAsync(topic);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetTopicOffsets/{topic}")]
        public async Task<IActionResult> GetTopicOffsets(string id, string topic)
        {
            var result = await _kafka.GetTopicOffsetsAsync(topic);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetConsumerGroups")]
        public async Task<IActionResult> GetConsumerGroups(string id)
        {
            var result = await _kafka.GetConsumerGroupsAsync();
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetConsumerGroupLag/{groupId}")]
        public async Task<IActionResult> GetConsumerGroupLag(string id, string groupId)
        {
            var result = await _kafka.GetConsumerLagAsync(groupId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetConsumerGroupByTopic/{topic}")]
        public async Task<IActionResult> GetConsumerGroupByTopic(string id, string topic)
        {
            var result = await _kafka.GetConsumerGroupsForTopicAsync(topic);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetTopicLag/{topic}")]
        public async Task<IActionResult> GetTopicLag(string id, string topic)
        {
            var result = await _kafka.GetTopicLagAsync(topic);
            return Ok(result);
        }
    }
}
