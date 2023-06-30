using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class PaymentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventRawJObject",
                table: "Payments",
                newName: "Message");

            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Payments",
                newName: "EventRawJObject");
        }
    }
}
