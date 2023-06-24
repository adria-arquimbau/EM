using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsManager.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAdminResponse = table.Column<bool>(type: "bit", nullable: false),
                    RespondedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketResponses_AspNetUsers_RespondedById",
                        column: x => x.RespondedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketResponses_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponses_RespondedById",
                table: "TicketResponses",
                column: "RespondedById");

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponses_TicketId",
                table: "TicketResponses",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketResponses");
        }
    }
}
