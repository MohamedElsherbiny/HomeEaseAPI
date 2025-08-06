namespace HomeEase.Application.DTOs.ProviderSchedule;

public class WorkingHoursDto
{
    public Guid? Id { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsOpen { get; set; }
}
