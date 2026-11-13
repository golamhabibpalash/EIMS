using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class uniqueConstrainModifiedForparamBusConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParamBusConfigs_ParamSL_ConfigName",
                table: "ParamBusConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_ParamBusConfigs_ConfigName",
                table: "ParamBusConfigs",
                column: "ConfigName",
                unique: true,
                filter: "[ConfigName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParamBusConfigs_ParamSL",
                table: "ParamBusConfigs",
                column: "ParamSL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParamBusConfigs_ConfigName",
                table: "ParamBusConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ParamBusConfigs_ParamSL",
                table: "ParamBusConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_ParamBusConfigs_ParamSL_ConfigName",
                table: "ParamBusConfigs",
                columns: new[] { "ParamSL", "ConfigName" },
                unique: true,
                filter: "[ConfigName] IS NOT NULL");
        }
    }
}
