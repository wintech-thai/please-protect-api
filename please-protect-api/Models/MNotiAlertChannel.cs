using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;
using System.Diagnostics.CodeAnalysis;

namespace Its.PleaseProtect.Api.Models
{
    [ExcludeFromCodeCoverage]
    [Table("NotiAlertChannels")]
    [Index(nameof(ChannelName), IsUnique = false)]
    [Index(nameof(Description), IsUnique = false)]
    [Index(nameof(Tags), IsUnique = false)]
    [Index(nameof(Type), IsUnique = false)]
    [Index(nameof(Status), IsUnique = false)]

    public class MNotiAlertChannel
    {
        [Key]
        [Column("noti_channel_id")]
        public Guid? Id { get; set; }
    
        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("channel_name")]
        public string? ChannelName { get; set; }

        [Column("channel_description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("type")]
        public string? Type { get; set; } /* Discord, Slack, Email, LINE, Webhook */

        [Column("status")]
        public string? Status { get; set; } /* Enabled, Disabled */

        [Column("discord_webhook_url")]
        public string? DiscordWebhookUrl { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        public MNotiAlertChannel()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }
}
