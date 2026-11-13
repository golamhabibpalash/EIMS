using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationSettinsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Student_ClassRoll",
                table: "Student");

            migrationBuilder.DropIndex(
                name: "IX_Student_UniqueId",
                table: "Student");

            migrationBuilder.CreateTable(
                name: "ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Student_UniqueId_ClassRoll",
                table: "Student",
                columns: new[] { "UniqueId", "ClassRoll" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSettings_ShortName",
                table: "ApplicationSettings",
                column: "ShortName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationSettings");

            migrationBuilder.DropIndex(
                name: "IX_Student_UniqueId_ClassRoll",
                table: "Student");

            migrationBuilder.CreateIndex(
                name: "IX_Student_ClassRoll",
                table: "Student",
                column: "ClassRoll",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Student_UniqueId",
                table: "Student",
                column: "UniqueId",
                unique: true);
        }
    }
}
