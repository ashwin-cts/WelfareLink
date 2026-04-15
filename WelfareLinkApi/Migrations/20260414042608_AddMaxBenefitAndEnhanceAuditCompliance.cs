using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLinkApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxBenefitAndEnhanceAuditCompliance : Migration
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

            migrationBuilder.AddColumn<decimal>(
                name: "MaxBenefitPerCitizen",
                table: "Programs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ApplicationID",
                table: "ComplianceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BenefitID",
                table: "ComplianceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CitizenID",
                table: "ComplianceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisbursementID",
                table: "ComplianceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ComplianceRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "AuditLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "AuditLogs",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceRecords_Users_RaisedByUserId",
                table: "ComplianceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplianceRecords_Users_ResolvedByUserId",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "MaxBenefitPerCitizen",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ApplicationID",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "BenefitID",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "CitizenID",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "DisbursementID",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ComplianceRecords");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceRecords_Users_RaisedByUserId",
                table: "ComplianceRecords",
                column: "RaisedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceRecords_Users_ResolvedByUserId",
                table: "ComplianceRecords",
                column: "ResolvedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
