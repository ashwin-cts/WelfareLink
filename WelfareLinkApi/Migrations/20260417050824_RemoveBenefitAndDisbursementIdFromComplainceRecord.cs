using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLinkApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBenefitAndDisbursementIdFromComplainceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenefitID",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "DisbursementID",
                table: "ComplianceRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BenefitID",
                table: "ComplianceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisbursementID",
                table: "ComplianceRecords",
                type: "int",
                nullable: true);
        }
    }
}
