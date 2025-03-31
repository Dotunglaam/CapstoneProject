using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Scheduledetail
    {
        public int ScheduleDetailId { get; set; }
        public int TimeSlotId { get; set; }
        public int ActivityId { get; set; }
        public int LocationId { get; set; }
        public string? Note { get; set; }
        public string? Day { get; set; }
        public int ScheduleId { get; set; }
        public string? Weekdate { get; set; }

        public virtual Activity Activity { get; set; } = null!;
        public virtual Location Location { get; set; } = null!;
        public virtual Schedule Schedule { get; set; } = null!;
        public virtual TimeSlot TimeSlot { get; set; } = null!;
    }
}
