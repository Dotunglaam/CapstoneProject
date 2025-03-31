using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class MenuHasGrade
    {
        public int MenuId { get; set; }
        public int GradeId { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual Grade Grade { get; set; } = null!;
        public virtual Menu Menu { get; set; } = null!;
    }
}
