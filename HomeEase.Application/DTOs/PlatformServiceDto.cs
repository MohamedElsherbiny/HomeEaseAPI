using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.DTOs
{
    public class CreatePlatformServiceDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UpdatePlatformServiceDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BasePlatformServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
