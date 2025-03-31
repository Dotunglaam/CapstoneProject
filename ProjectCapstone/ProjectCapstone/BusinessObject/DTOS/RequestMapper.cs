using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class RequestMapper
    {
        public int RequestId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? StatusRequest { get; set; }
        public DateTime? CreateAt { get; set; }
        public int CreateBy { get; set; }
        public int? StudentId { get; set; }
        public string? ProcessNote { get; set; }

    }
}
