using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class ActivityMapper
    {
        public int ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public int? Status { get; set; }
    }
}
