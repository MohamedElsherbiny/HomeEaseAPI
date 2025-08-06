using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.DTOs.PlatformService
{
    public class BasePlatformServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? NameAr { get; set; }
        public string Description { get; set; }
        public string? DescriptionAr { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
