using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventPriceNullDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPrice_Events_EventId",
                table: "EventPrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPrice",
                table: "EventPrice");

            migrationBuilder.RenameTable(
                name: "EventPrice",
                newName: "EventPrices");

            migrationBuilder.RenameIndex(
                name: "IX_EventPrice_EventId",
                table: "EventPrices",
                newName: "IX_EventPrices_EventId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "EventPrices",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPrices",
                table: "EventPrices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPrices_Events_EventId",
                table: "EventPrices",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPrices_Events_EventId",
                table: "EventPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPrices",
                table: "EventPrices");

            migrationBuilder.RenameTable(
                name: "EventPrices",
                newName: "EventPrice");

            migrationBuilder.RenameIndex(
                name: "IX_EventPrices_EventId",
                table: "EventPrice",
                newName: "IX_EventPrice_EventId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "EventPrice",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPrice",
                table: "EventPrice",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPrice_Events_EventId",
                table: "EventPrice",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
