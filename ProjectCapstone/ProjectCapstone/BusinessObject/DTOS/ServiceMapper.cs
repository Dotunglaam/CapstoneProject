using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class ServiceMapper
    {
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public string? ServiceDes { get; set; }
        public int CategoryServiceId { get; set; }
        public int SchoolId { get; set; }
        public int? Status { get; set; }


        public virtual CategoryServiceMapper? CategoryService { get; set; }
    }
    public class ServiceMapper1
    {
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public string? ServiceDes { get; set; }
        public int CategoryServiceId { get; set; }
        public int SchoolId { get; set; }
        public int? Status { get; set; }
    }
}
