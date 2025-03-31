using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Schedule
    {
        public Schedule()
        {
            Scheduledetails = new HashSet<Scheduledetail>();
        }

        public int ScheduleId { get; set; }
        public int? Status { get; set; }
        public int ClassId { get; set; }
        public string? TeacherName { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Scheduledetail> Scheduledetails { get; set; }
    }
}
