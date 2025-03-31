using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Menu
    {
        public Menu()
        {
            MenuHasGrades = new HashSet<MenuHasGrade>();
            Menudetails = new HashSet<Menudetail>();
        }

        public int MenuId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int SchoolId { get; set; }
        public int? Status { get; set; }

        public virtual School School { get; set; } = null!;
        public virtual ICollection<MenuHasGrade> MenuHasGrades { get; set; }
        public virtual ICollection<Menudetail> Menudetails { get; set; }
    }
}
