using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Semester
    {
        public Semester()
        {
            Classes = new HashSet<Class>();
            Semesterreals = new HashSet<Semesterreal>();
            Tuitions = new HashSet<Tuition>();
        }

        public int SemesterId { get; set; }
        public string? Name { get; set; }
        public int? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Semesterreal> Semesterreals { get; set; }
        public virtual ICollection<Tuition> Tuitions { get; set; }
    }
}
