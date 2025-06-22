using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageType",
                table: "ProviderImages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "ProviderImages");
        }
    }
}
