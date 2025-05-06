using System;
using Massage.Domain.Enums;

namespace Massage.Domain.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ServiceType ServiceType { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Provider Provider { get; set; }
    }
}