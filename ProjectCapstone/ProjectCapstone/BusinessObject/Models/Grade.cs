using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Grade
    {
        public Grade()
        {
            Children = new HashSet<Child>();
            Classes = new HashSet<Class>();
            MenuHasGrades = new HashSet<MenuHasGrade>();
        }

        public int GradeId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? BaseTuitionFee { get; set; }

        public virtual ICollection<Child> Children { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<MenuHasGrade> MenuHasGrades { get; set; }
    }
}
