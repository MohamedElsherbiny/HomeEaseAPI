using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
               name: "AppointmentDateTime",
               table: "Bookings",
               type: "datetime2",
               nullable: false,
               defaultValue: DateTime.Now); // or use a more reasonable default
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
               name: "AppointmentDateTime",
               table: "Bookings");
        }
    }
}
