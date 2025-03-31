using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class ClassHasChild
    {
        public int ClassId { get; set; }
        public int StudentId { get; set; }
        public DateTime? Date { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual Child Student { get; set; } = null!;
    }
}
