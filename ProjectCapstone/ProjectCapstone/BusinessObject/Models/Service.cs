using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Service
    {
        public Service()
        {
            Checkservices = new HashSet<Checkservice>();
            ChildrenHasServices = new HashSet<ChildrenHasService>();
            Payments = new HashSet<Payment>();
        }

        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public string? ServiceDes { get; set; }
        public int SchoolId { get; set; }
        public int CategoryServiceId { get; set; }
        public int? Status { get; set; }

        public virtual Cagetoryservice CategoryService { get; set; } = null!;
        public virtual School School { get; set; } = null!;
        public virtual ICollection<Checkservice> Checkservices { get; set; }
        public virtual ICollection<ChildrenHasService> ChildrenHasServices { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
