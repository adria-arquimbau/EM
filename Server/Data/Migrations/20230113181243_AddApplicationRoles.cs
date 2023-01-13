using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "c0781463-f652-4d6e-beec-7fd27e67968f", null, "User", "USER" },
                    { "c23855e6-e537-4d6c-a529-58a46e7d32b6", null, "Administrator", "ADMINISTRATOR" },
                    { "e62edec8-0a55-48db-b611-5276d360a1fc", null, "Organizer", "ORGANIZER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c0781463-f652-4d6e-beec-7fd27e67968f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c23855e6-e537-4d6c-a529-58a46e7d32b6");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e62edec8-0a55-48db-b611-5276d360a1fc");
        }
    }
}
