using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WelfareLink.CitizenManagement.API.Migrations
{
    public partial class AddCitizenUserRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // We removed all CreateTable calls because the tables already exist.
            // We only keep the lines that create the relationship.

            // 1. Create the Index for the Foreign Key
            migrationBuilder.CreateIndex(
                name: "IX_Citizens_UserId",
                table: "Citizens",
                column: "UserId");

            // 2. Add the actual Foreign Key constraint linking Citizen to User
            migrationBuilder.AddForeignKey(
                name: "FK_Citizens_Users_UserId",
                table: "Citizens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the relationship if we roll back
            migrationBuilder.DropForeignKey(
                name: "FK_Citizens_Users_UserId",
                table: "Citizens");

            migrationBuilder.DropIndex(
                name: "IX_Citizens_UserId",
                table: "Citizens");
        }
    }
}