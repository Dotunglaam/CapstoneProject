using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class LocationMapper
    {
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public int? Status { get; set; }
    }
}
