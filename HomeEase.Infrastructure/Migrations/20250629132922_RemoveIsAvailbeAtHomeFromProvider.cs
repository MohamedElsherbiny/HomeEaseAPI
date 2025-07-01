using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsAvailbeAtHomeFromProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailableAtCenter",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsAvailableAtHome",
                table: "Services");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableAtCenter",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableAtHome",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
