using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class TimeSlot
    {
        public TimeSlot()
        {
            Scheduledetails = new HashSet<Scheduledetail>();
        }

        public int TimeSlotId { get; set; }
        public string? TimeName { get; set; }

        public virtual ICollection<Scheduledetail> Scheduledetails { get; set; }
    }
}
