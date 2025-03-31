using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Cagetoryservice
    {
        public Cagetoryservice()
        {
            Services = new HashSet<Service>();
        }

        public int CategoryServiceId { get; set; }
        public string? CategoryName { get; set; }

        public virtual ICollection<Service> Services { get; set; }
    }
}
