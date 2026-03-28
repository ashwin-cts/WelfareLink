using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLink.Migrations
{
    /// <inheritdoc />
    public partial class foriegnKeyMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BenefitID",
                table: "WelfareApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WelfareProgramsProgramID",
                table: "WelfareApplications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WelfareApplications_BenefitID",
                table: "WelfareApplications",
                column: "BenefitID");

            migrationBuilder.CreateIndex(
                name: "IX_WelfareApplications_WelfareProgramsProgramID",
                table: "WelfareApplications",
                column: "WelfareProgramsProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_WelfareApplications_Benefits_BenefitID",
                table: "WelfareApplications",
                column: "BenefitID",
                principalTable: "Benefits",
                principalColumn: "BenefitID");

            migrationBuilder.AddForeignKey(
                name: "FK_WelfareApplications_Programs_WelfareProgramsProgramID",
                table: "WelfareApplications",
                column: "WelfareProgramsProgramID",
                principalTable: "Programs",
                principalColumn: "ProgramID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WelfareApplications_Benefits_BenefitID",
                table: "WelfareApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_WelfareApplications_Programs_WelfareProgramsProgramID",
                table: "WelfareApplications");

            migrationBuilder.DropIndex(
                name: "IX_WelfareApplications_BenefitID",
                table: "WelfareApplications");

            migrationBuilder.DropIndex(
                name: "IX_WelfareApplications_WelfareProgramsProgramID",
                table: "WelfareApplications");

            migrationBuilder.DropColumn(
                name: "BenefitID",
                table: "WelfareApplications");

            migrationBuilder.DropColumn(
                name: "WelfareProgramsProgramID",
                table: "WelfareApplications");
        }
    }
}
