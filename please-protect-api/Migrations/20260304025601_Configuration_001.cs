using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otep.api.Migrations
{
    /// <inheritdoc />
    public partial class Configuration_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    config_type = table.Column<string>(type: "text", nullable: true),
                    config_value = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.config_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_config_type",
                table: "Configurations",
                column: "config_type");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_config_value",
                table: "Configurations",
                column: "config_value");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_org_id_config_type",
                table: "Configurations",
                columns: new[] { "org_id", "config_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");
        }
    }
}
