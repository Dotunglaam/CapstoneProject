using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class School
    {
        public School()
        {
            Classes = new HashSet<Class>();
            Menus = new HashSet<Menu>();
            Services = new HashSet<Service>();
        }

        public int SchoolId { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? SchoolDes { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}
