using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otep.api.Migrations
{
    /// <inheritdoc />
    public partial class NotAlertEvent_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotiAlertEvents",
                columns: table => new
                {
                    noti_alert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true),
                    detail = table.Column<string>(type: "text", nullable: true),
                    severity = table.Column<string>(type: "text", nullable: true),
                    raw_data = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotiAlertEvents", x => x.noti_alert_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertEvents_detail",
                table: "NotiAlertEvents",
                column: "detail");

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertEvents_name",
                table: "NotiAlertEvents",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertEvents_severity",
                table: "NotiAlertEvents",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "IX_NotiAlertEvents_summary",
                table: "NotiAlertEvents",
                column: "summary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotiAlertEvents");
        }
    }
}
