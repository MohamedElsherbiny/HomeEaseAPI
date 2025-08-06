namespace HomeEase.Application.DTOs.Booking;

public class BookingStatisticsDto
{
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalRevenue { get; set; }

    public Dictionary<string, int> BookingsByBasePlatformService { get; set; }
    public Dictionary<string, Dictionary<string, int>> BookingsByStatusAndMonth { get; set; }
}
