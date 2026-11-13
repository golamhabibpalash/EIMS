using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    public partial class smsSetupModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AbsentNotificationEmployee",
                table: "SetupMobileSMSs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AbsentNotificationStudent",
                table: "SetupMobileSMSs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsentNotificationEmployee",
                table: "SetupMobileSMSs");

            migrationBuilder.DropColumn(
                name: "AbsentNotificationStudent",
                table: "SetupMobileSMSs");
        }
    }
}
