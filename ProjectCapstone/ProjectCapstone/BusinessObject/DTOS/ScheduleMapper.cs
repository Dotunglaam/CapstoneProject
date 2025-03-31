using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class ScheduleMapper
    {
        public int ScheduleId { get; set; }
        public int? Status { get; set; }
        public int ClassId { get; set; }
        public string? TeacherName { get; set; }
    }
}
