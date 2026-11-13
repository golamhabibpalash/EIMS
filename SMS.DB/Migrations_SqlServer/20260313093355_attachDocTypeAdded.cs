using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class attachDocTypeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttachDocTypeId",
                table: "AttachDocs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AttachDocType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocTypeFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachDocType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttachDocs_AttachDocTypeId",
                table: "AttachDocs",
                column: "AttachDocTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttachDocs_AttachDocType_AttachDocTypeId",
                table: "AttachDocs",
                column: "AttachDocTypeId",
                principalTable: "AttachDocType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttachDocs_AttachDocType_AttachDocTypeId",
                table: "AttachDocs");

            migrationBuilder.DropTable(
                name: "AttachDocType");

            migrationBuilder.DropIndex(
                name: "IX_AttachDocs_AttachDocTypeId",
                table: "AttachDocs");

            migrationBuilder.DropColumn(
                name: "AttachDocTypeId",
                table: "AttachDocs");
        }
    }
}
