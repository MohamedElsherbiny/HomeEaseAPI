namespace HomeEase.Application.DTOs.ProviderSchedule;

public class SpecialDateDto
{
    public Guid? Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsClosed { get; set; }
    public string Note { get; set; }
}
