using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class ScheduleDetailMapper
    {
        public int ScheduleDetailId { get; set; }
        public int TimeSlotId { get; set; }
        public int ActivityId { get; set; }
        public int LocationId { get; set; }
        public string? Note { get; set; }
        public string? Day { get; set; }
        public int ScheduleId { get; set; }
        public string? Weekdate { get; set; }

        public string? TimeSlotName { get; set; }
        public string? ActivityName { get; set; }
        public string? LocationName { get; set; }
    }
}
