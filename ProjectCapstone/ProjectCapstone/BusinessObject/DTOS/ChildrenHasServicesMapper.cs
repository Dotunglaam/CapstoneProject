using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class ChildrenHasServicesMapper
    {

            public int ChidrenServicesId { get; set; }
            public DateOnly? Time { get; set; }
            public int? Status { get; set; }
            public int ServiceId { get; set; }
            public int StudentId { get; set; }

    }
}
