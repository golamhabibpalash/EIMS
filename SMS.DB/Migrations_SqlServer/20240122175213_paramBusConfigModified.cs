using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class paramBusConfigModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParamBusConfigs_ParamSL",
                table: "ParamBusConfigs");

            migrationBuilder.AddColumn<string>(
                name: "ConfigName",
                table: "ParamBusConfigs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParamBusConfigs_ParamSL_ConfigName",
                table: "ParamBusConfigs",
                columns: new[] { "ParamSL", "ConfigName" },
                unique: true,
                filter: "[ConfigName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParamBusConfigs_ParamSL_ConfigName",
                table: "ParamBusConfigs");

            migrationBuilder.DropColumn(
                name: "ConfigName",
                table: "ParamBusConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_ParamBusConfigs_ParamSL",
                table: "ParamBusConfigs",
                column: "ParamSL",
                unique: true);
        }
    }
}
