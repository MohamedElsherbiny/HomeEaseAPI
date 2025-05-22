namespace Massage.Domain.Entities;

public class Schedule
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Provider? Provider { get; set; }
}
