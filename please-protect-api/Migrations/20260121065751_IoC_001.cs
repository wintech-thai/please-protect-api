using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otep.api.Migrations
{
    /// <inheritdoc />
    public partial class IoC_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Iocs",
                columns: table => new
                {
                    ioc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    dataset = table.Column<string>(type: "text", nullable: true),
                    ioc_type = table.Column<string>(type: "text", nullable: true),
                    ioc_sub_type = table.Column<string>(type: "text", nullable: true),
                    ioc_value = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_seen_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iocs", x => x.ioc_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_dataset",
                table: "Iocs",
                column: "dataset");

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_ioc_sub_type",
                table: "Iocs",
                column: "ioc_sub_type");

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_ioc_type",
                table: "Iocs",
                column: "ioc_type");

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_ioc_value",
                table: "Iocs",
                column: "ioc_value");

            migrationBuilder.CreateIndex(
                name: "IX_Iocs_org_id_ioc_type_dataset_ioc_value_ioc_sub_type",
                table: "Iocs",
                columns: new[] { "org_id", "ioc_type", "dataset", "ioc_value", "ioc_sub_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Iocs");
        }
    }
}
