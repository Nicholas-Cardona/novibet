using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novibet.Data.Migrations
{
    /// <inheritdoc />
    public partial class WalletEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Wallets",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Wallets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Wallets");
        }
    }
}
