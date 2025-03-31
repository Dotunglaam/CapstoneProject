using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Request
    {
        public int RequestId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? StatusRequest { get; set; }
        public DateTime? CreateAt { get; set; }
        public int CreateBy { get; set; }
        public int? StudentId { get; set; }
        public string? ProcessNote { get; set; }

        public virtual User CreateByNavigation { get; set; } = null!;
    }
}
