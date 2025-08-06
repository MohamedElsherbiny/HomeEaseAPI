using HomeEase.Application.DTOs.ProviderSchedule;

namespace HomeEase.Application.DTOs.Provider;

public class ProviderScheduleDto
{
    public List<WorkingHoursDto> RegularHours { get; set; }
    public List<SpecialDateDto> SpecialDates { get; set; }
    public List<TimeSlotDto> AvailableSlots { get; set; }
}
