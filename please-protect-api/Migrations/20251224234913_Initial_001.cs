using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otep.api.Migrations
{
    /// <inheritdoc />
    public partial class Initial_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasterRefs",
                columns: table => new
                {
                    master_ref_id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    ref_type = table.Column<int>(type: "integer", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterRefs", x => x.master_ref_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MasterRefs_org_id",
                table: "MasterRefs",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "IX_MasterRefs_org_id_code",
                table: "MasterRefs",
                columns: new[] { "org_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterRefs");
        }
    }
}
