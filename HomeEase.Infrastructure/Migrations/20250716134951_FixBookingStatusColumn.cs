﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
            name: "Status",
            table: "Bookings",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
            name: "Status",
            table: "Bookings",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");
        }
    }
}
