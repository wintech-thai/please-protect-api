using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otep.api.Migrations
{
    /// <inheritdoc />
    public partial class Subnet_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subnets",
                columns: table => new
                {
                    subnet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    cidr = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subnets", x => x.subnet_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_cidr",
                table: "Subnets",
                column: "cidr");

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_name",
                table: "Subnets",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_org_id_cidr",
                table: "Subnets",
                columns: new[] { "org_id", "cidr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subnets_org_id_name",
                table: "Subnets",
                columns: new[] { "org_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subnets");
        }
    }
}
