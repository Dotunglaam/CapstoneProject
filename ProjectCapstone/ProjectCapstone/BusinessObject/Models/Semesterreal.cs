using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Semesterreal
    {
        public int SemesterrealId { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? SemesterId { get; set; }

        public virtual Semester? Semester { get; set; }
    }
}
