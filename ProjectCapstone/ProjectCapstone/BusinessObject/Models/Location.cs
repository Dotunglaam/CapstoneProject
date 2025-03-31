using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Location
    {
        public Location()
        {
            Scheduledetails = new HashSet<Scheduledetail>();
        }

        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public int? Status { get; set; }

        public virtual ICollection<Scheduledetail> Scheduledetails { get; set; }
    }
}
