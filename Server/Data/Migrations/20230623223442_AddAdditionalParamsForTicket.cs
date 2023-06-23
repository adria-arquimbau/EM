using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalParamsForTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SolvedById",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SolvedDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SolvedById",
                table: "Tickets",
                column: "SolvedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_SolvedById",
                table: "Tickets",
                column: "SolvedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_SolvedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SolvedById",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SolvedById",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SolvedDate",
                table: "Tickets");
        }
    }
}
