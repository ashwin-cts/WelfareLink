using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLink.Migrations
{
    /// <inheritdoc />
    public partial class foriegnKeyMap1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WelfareApplications_Programs_WelfareProgramsProgramID",
                table: "WelfareApplications");

            migrationBuilder.DropIndex(
                name: "IX_WelfareApplications_WelfareProgramsProgramID",
                table: "WelfareApplications");

            migrationBuilder.DropColumn(
                name: "WelfareProgramsProgramID",
                table: "WelfareApplications");

            migrationBuilder.CreateIndex(
                name: "IX_WelfareApplications_ProgramID",
                table: "WelfareApplications",
                column: "ProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_WelfareApplications_Programs_ProgramID",
                table: "WelfareApplications",
                column: "ProgramID",
                principalTable: "Programs",
                principalColumn: "ProgramID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WelfareApplications_Programs_ProgramID",
                table: "WelfareApplications");

            migrationBuilder.DropIndex(
                name: "IX_WelfareApplications_ProgramID",
                table: "WelfareApplications");

            migrationBuilder.AddColumn<int>(
                name: "WelfareProgramsProgramID",
                table: "WelfareApplications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WelfareApplications_WelfareProgramsProgramID",
                table: "WelfareApplications",
                column: "WelfareProgramsProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_WelfareApplications_Programs_WelfareProgramsProgramID",
                table: "WelfareApplications",
                column: "WelfareProgramsProgramID",
                principalTable: "Programs",
                principalColumn: "ProgramID");
        }
    }
}
