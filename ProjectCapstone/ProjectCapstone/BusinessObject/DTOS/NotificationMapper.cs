using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class NotificationMapper
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public int RoleId { get; set; }
        public virtual ICollection<UsernotificationMapper> Usernotifications { get; set; }
    }

    public class UsernotificationMapper
    {
        public int UserNotificationId { get; set; }
        public int UserId { get; set; }
        public int NotificationId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ReadAt { get; set; }
    }
}
