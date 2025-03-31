using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Notification
    {
        public Notification()
        {
            Usernotifications = new HashSet<Usernotification>();
        }

        public int NotificationId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public int RoleId { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Usernotification> Usernotifications { get; set; }
    }
}
