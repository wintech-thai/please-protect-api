using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otep.api.Migrations
{
    /// <inheritdoc />
    public partial class NotiAlertChannel_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotiAlertChannels",
                columns: table => new
                {
                    noti_channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    channel_name = table.Column<string>(type: "text", nullable: true),
                    channel_description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    discord_webhook_url = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotiAlertChannels", x => x.noti_channel_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertChannels_channel_description",
                table: "NotiAlertChannels",
                column: "channel_description");

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertChannels_channel_name",
                table: "NotiAlertChannels",
                column: "channel_name");

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertChannels_org_id_channel_name",
                table: "NotiAlertChannels",
                columns: new[] { "org_id", "channel_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertChannels_status",
                table: "NotiAlertChannels",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertChannels_tags",
                table: "NotiAlertChannels",
                column: "tags");

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertChannels_type",
                table: "NotiAlertChannels",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotiAlertChannels");
        }
    }
}
