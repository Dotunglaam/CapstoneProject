using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Checkservice
    {
        public int CheckServiceId { get; set; }
        public int ServiceId { get; set; }
        public DateOnly? Date { get; set; }
        public int StudentId { get; set; }
        public int? Status { get; set; }
        public int? PayService { get; set; }
        public DateOnly? DatePayService { get; set; }

        public virtual Service Service { get; set; } = null!;
        public virtual Child Student { get; set; } = null!;
    }
}
