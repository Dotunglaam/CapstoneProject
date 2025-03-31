using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class ChildrenHasService
    {
        public int ChidrenServicesId { get; set; }
        public DateOnly? Time { get; set; }
        public int? Status { get; set; }
        public int ServiceId { get; set; }
        public int StudentId { get; set; }

        public virtual Service Service { get; set; } = null!;
        public virtual Child Student { get; set; } = null!;
    }
}
