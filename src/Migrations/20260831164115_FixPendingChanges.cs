using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApi.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_UserDto_ClientId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_UserDto_DeliveryId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "UserDto");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClientId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "DeliveryId",
                table: "Orders",
                newName: "Delivery_UserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Orders",
                newName: "Client_UserId");

            migrationBuilder.AddColumn<string>(
                name: "Client_Username",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Delivery_Username",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Client_Username",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Delivery_Username",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Delivery_UserId",
                table: "Orders",
                newName: "DeliveryId");

            migrationBuilder.RenameColumn(
                name: "Client_UserId",
                table: "Orders",
                newName: "ClientId");

            migrationBuilder.CreateTable(
                name: "UserDto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientId",
                table: "Orders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryId",
                table: "Orders",
                column: "DeliveryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_UserDto_ClientId",
                table: "Orders",
                column: "ClientId",
                principalTable: "UserDto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_UserDto_DeliveryId",
                table: "Orders",
                column: "DeliveryId",
                principalTable: "UserDto",
                principalColumn: "Id");
        }
    }
}
