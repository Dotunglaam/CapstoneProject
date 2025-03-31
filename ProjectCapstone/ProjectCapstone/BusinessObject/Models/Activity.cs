using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Activity
    {
        public Activity()
        {
            Scheduledetails = new HashSet<Scheduledetail>();
        }

        public int ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public int? Status { get; set; }

        public virtual ICollection<Scheduledetail> Scheduledetails { get; set; }
    }
}
