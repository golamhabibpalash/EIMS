using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class ModuleSubmoduleAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModuleName",
                table: "ClaimStores");

            migrationBuilder.DropColumn(
                name: "SubModuleName",
                table: "ClaimStores");

            migrationBuilder.AddColumn<int>(
                name: "SubModuleId",
                table: "ClaimStores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProjectModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSubModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubModuleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectModuleId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSubModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSubModules_ProjectModules_ProjectModuleId",
                        column: x => x.ProjectModuleId,
                        principalTable: "ProjectModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimStores_SubModuleId",
                table: "ClaimStores",
                column: "SubModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSubModules_ProjectModuleId",
                table: "ProjectSubModules",
                column: "ProjectModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimStores_ProjectSubModules_SubModuleId",
                table: "ClaimStores",
                column: "SubModuleId",
                principalTable: "ProjectSubModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimStores_ProjectSubModules_SubModuleId",
                table: "ClaimStores");

            migrationBuilder.DropTable(
                name: "ProjectSubModules");

            migrationBuilder.DropTable(
                name: "ProjectModules");

            migrationBuilder.DropIndex(
                name: "IX_ClaimStores_SubModuleId",
                table: "ClaimStores");

            migrationBuilder.DropColumn(
                name: "SubModuleId",
                table: "ClaimStores");

            migrationBuilder.AddColumn<string>(
                name: "ModuleName",
                table: "ClaimStores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubModuleName",
                table: "ClaimStores",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
