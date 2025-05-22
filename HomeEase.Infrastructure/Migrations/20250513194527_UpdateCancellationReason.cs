using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCancellationReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // احذف العمود Rating القديم
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Reviews");

            // أضف العمود Rating الجديد كـ decimal
            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Reviews",
                type: "decimal(18,2)",
                nullable: true);

            // اجعل CancellationReason nullable
            migrationBuilder.AlterColumn<string>(
                name: "CancellationReason",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // احذف العمود Rating الجديد
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Reviews");

            // أعد العمود Rating القديم كـ Guid
            migrationBuilder.AddColumn<Guid>(
                name: "Rating",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            // أعد CancellationReason كـ not null
            migrationBuilder.AlterColumn<string>(
                name: "CancellationReason",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

        }

    }
}
