using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class AttendanceMapper
    {
        public int AttendanceId { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int ClassId { get; set; }
        public List<AttendanceDetailMapper> AttendanceDetail { get; set; }
    }
    public class AttendanceDetailMapper
    {
        public int AttendanceDetailId { get; set; }
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class AttendanceInfo
    {
        public DateTime CreatedAt { get; set; }
        public int ClassID { get; set; }
        public string Type { get; set; }

    }
}