using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Teacher
    {
        public Teacher()
        {
            Classes = new HashSet<Class>();
        }

        public int TeacherId { get; set; }
        public string? Name { get; set; }
        public string? Education { get; set; }
        public string? Experience { get; set; }
        public sbyte? HomeroomTeacher { get; set; }

        public virtual User TeacherNavigation { get; set; } = null!;

        public virtual ICollection<Class> Classes { get; set; }
    }
}
