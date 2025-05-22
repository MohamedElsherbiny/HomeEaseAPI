namespace HomeEase.Domain.Entities;

public class AvailabilitySlots
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Provider Provider { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime? SpecificDate { get; set; }
}
