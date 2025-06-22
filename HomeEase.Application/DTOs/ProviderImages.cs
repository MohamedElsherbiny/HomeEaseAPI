using HomeEase.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeEase.Application.DTOs
{
    public class ProviderImageDto
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public string ImageUrl { get; set; }
        public ImageType ImageType { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
