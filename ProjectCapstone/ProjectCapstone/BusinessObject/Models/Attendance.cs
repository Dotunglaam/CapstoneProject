using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Attendance
    {
        public Attendance()
        {
            AttendanceDetails = new HashSet<AttendanceDetail>();
        }

        public int AttendanceId { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int ClassId { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<AttendanceDetail> AttendanceDetails { get; set; }
    }
}
