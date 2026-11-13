using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.DB.Migrations
{
    /// <inheritdoc />
    public partial class GradingTableHistAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GradingTableHists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumberRangeMin = table.Column<int>(type: "int", nullable: false),
                    NumberRangeMax = table.Column<int>(type: "int", nullable: false),
                    LetterGrade = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    GradePoint = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gradeComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcademicExamGroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MACAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingTableHists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "studentPaymentSummerySMS_VMs",
                columns: table => new
                {
                    ResidentialPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonResidentialPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GradingTableHists");

            migrationBuilder.DropTable(
                name: "studentPaymentSummerySMS_VMs");
        }
    }
}
