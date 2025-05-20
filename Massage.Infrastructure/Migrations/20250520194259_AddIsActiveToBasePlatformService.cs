using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Massage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToBasePlatformService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BasePlatformService");

            
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BasePlatformService",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BasePlatformService");
        }
    }
}
