using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLink.Migrations
{
    /// <inheritdoc />
    public partial class updateForiegnKey2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WelfareApplications_Benefits_BenefitID",
                table: "WelfareApplications");

            migrationBuilder.DropIndex(
                name: "IX_WelfareApplications_BenefitID",
                table: "WelfareApplications");

            migrationBuilder.DropColumn(
                name: "BenefitID",
                table: "WelfareApplications");

            migrationBuilder.CreateIndex(
                name: "IX_Benefits_ApplicationID",
                table: "Benefits",
                column: "ApplicationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Benefits_WelfareApplications_ApplicationID",
                table: "Benefits",
                column: "ApplicationID",
                principalTable: "WelfareApplications",
                principalColumn: "ApplicationID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Benefits_WelfareApplications_ApplicationID",
                table: "Benefits");

            migrationBuilder.DropIndex(
                name: "IX_Benefits_ApplicationID",
                table: "Benefits");

            migrationBuilder.AddColumn<int>(
                name: "BenefitID",
                table: "WelfareApplications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WelfareApplications_BenefitID",
                table: "WelfareApplications",
                column: "BenefitID");

            migrationBuilder.AddForeignKey(
                name: "FK_WelfareApplications_Benefits_BenefitID",
                table: "WelfareApplications",
                column: "BenefitID",
                principalTable: "Benefits",
                principalColumn: "BenefitID");
        }
    }
}
