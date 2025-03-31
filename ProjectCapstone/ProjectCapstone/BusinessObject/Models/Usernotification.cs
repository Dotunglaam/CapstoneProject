using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Usernotification
    {
        public int UserNotificationId { get; set; }
        public int UserId { get; set; }
        public int NotificationId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ReadAt { get; set; }

        public virtual Notification Notification { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
