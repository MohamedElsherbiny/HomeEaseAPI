using System;
using HomeEase.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    public partial class AddColumnsinProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "Providers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Providers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Providers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }
       
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Providers");

        }
    }
}
