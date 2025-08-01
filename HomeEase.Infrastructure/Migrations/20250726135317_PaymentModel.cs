using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PaymentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookingId1",
                table: "PaymentInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PaymentInfos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                table: "PaymentInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "PaymentInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentUrl",
                table: "PaymentInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "PaymentInfos",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "PaymentInfos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TapChargeId",
                table: "PaymentInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TapPaymentId",
                table: "PaymentInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WebhookData",
                table: "PaymentInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_BookingId1",
                table: "PaymentInfos",
                column: "BookingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInfos_Bookings_BookingId1",
                table: "PaymentInfos",
                column: "BookingId1",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInfos_Bookings_BookingId1",
                table: "PaymentInfos");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInfos_BookingId1",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "BookingId1",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "PaymentUrl",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "TapChargeId",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "TapPaymentId",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "WebhookData",
                table: "PaymentInfos");
        }
    }
}
