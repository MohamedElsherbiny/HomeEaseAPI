using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBasePlatformServiceRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BasePlatformServiceId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_BasePlatformServiceId",
                table: "Services",
                column: "BasePlatformServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_BasePlatformService_BasePlatformServiceId",
                table: "Services",
                column: "BasePlatformServiceId",
                principalTable: "BasePlatformService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_BasePlatformService_BasePlatformServiceId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_BasePlatformServiceId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "BasePlatformServiceId",
                table: "Services");
        }
    }
}
