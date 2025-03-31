using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class AttendanceDetail
    {
        public int AttendanceDetailId { get; set; }
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public virtual Attendance Attendance { get; set; } = null!;
        public virtual Child Student { get; set; } = null!;
    }
}
