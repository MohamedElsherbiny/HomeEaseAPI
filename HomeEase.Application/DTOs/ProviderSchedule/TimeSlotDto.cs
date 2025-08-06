namespace HomeEase.Application.DTOs.ProviderSchedule;

public class TimeSlotDto
{
    public Guid? Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
