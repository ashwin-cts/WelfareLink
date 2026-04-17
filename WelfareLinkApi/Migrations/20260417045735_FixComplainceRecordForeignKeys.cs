using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLinkApi.Migrations
{
    /// <inheritdoc />
    public partial class FixComplainceRecordForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceRecords_Users_RaisedByUserId",
                table: "ComplianceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceRecords_Users_ResolvedByUserId",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ComplianceRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceRecords_Users_RaisedByUserId",
                table: "ComplianceRecords",
                column: "RaisedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceRecords_Users_ResolvedByUserId",
                table: "ComplianceRecords",
                column: "ResolvedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceRecords_Users_RaisedByUserId",
                table: "ComplianceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceRecords_Users_ResolvedByUserId",
                table: "ComplianceRecords");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ComplianceRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceRecords_Users_RaisedByUserId",
                table: "ComplianceRecords",
                column: "RaisedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceRecords_Users_ResolvedByUserId",
                table: "ComplianceRecords",
                column: "ResolvedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
