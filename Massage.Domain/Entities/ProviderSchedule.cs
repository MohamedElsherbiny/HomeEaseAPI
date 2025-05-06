using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Domain.Entities
{
    public class ProviderSchedule
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }

        // Navigation properties
        public virtual Provider? Provider { get; set; }
        public virtual ICollection<WorkingHours> RegularHours { get; set; }
        public virtual ICollection<SpecialDate> SpecialDates { get; set; }
        public virtual ICollection<TimeSlot> AvailableSlots { get; set; }
    }

    public class WorkingHours
    {
        public Guid Id { get; set; }
        public Guid ProviderScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOpen { get; set; }

        // Navigation property
        public virtual ProviderSchedule ProviderSchedule { get; set; }
    }

    public class SpecialDate
    {
        public Guid Id { get; set; }
        public Guid ProviderScheduleId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsClosed { get; set; }
        public string Note { get; set; }

        // Navigation property
        public virtual ProviderSchedule ProviderSchedule { get; set; }
    }

    public class TimeSlot
    {
        public Guid Id { get; set; }
        public Guid ProviderScheduleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }

        // Navigation property
        public virtual ProviderSchedule ProviderSchedule { get; set; }
    }
}
