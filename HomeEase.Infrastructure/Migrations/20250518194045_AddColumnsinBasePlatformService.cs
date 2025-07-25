﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsinBasePlatformService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BasePlatformService",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BasePlatformService");
        }
    }
}
