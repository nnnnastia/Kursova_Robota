using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kursova_Robota.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AgreePrivacyPolicy",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Subscribe",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgreePrivacyPolicy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Subscribe",
                table: "Users");
        }
    }
}
