using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class CheckservicesMapper
    {
        public int CheckServiceId { get; set; }
        public int ServiceId { get; set; }
        public DateOnly? Date { get; set; }
        public int StudentId { get; set; }
        public int? Status { get; set; }

        public int? PayService { get; set; }

    }
}
