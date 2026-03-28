using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLink.Migrations
{
    /// <inheritdoc />
    public partial class foriegnkeyestablish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EligibilityChecks_WelfareApplications_ApplicationID",
                table: "EligibilityChecks");

            migrationBuilder.AddForeignKey(
                name: "FK_EligibilityChecks_WelfareApplications_ApplicationID",
                table: "EligibilityChecks",
                column: "ApplicationID",
                principalTable: "WelfareApplications",
                principalColumn: "ApplicationID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EligibilityChecks_WelfareApplications_ApplicationID",
                table: "EligibilityChecks");

            migrationBuilder.AddForeignKey(
                name: "FK_EligibilityChecks_WelfareApplications_ApplicationID",
                table: "EligibilityChecks",
                column: "ApplicationID",
                principalTable: "WelfareApplications",
                principalColumn: "ApplicationID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
