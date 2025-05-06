using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massage.Domain.Entities
{
    public class UserPreferences
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public string PreferredCurrency { get; set; } = "USD";
        public string[] FavoriteServiceTypes { get; set; }

        public virtual User User { get; set; }
    }
}
