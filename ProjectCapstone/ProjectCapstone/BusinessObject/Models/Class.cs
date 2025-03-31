using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Class
    {
        public Class()
        {
            Albums = new HashSet<Album>();
            Attendances = new HashSet<Attendance>();
            ClassHasChildren = new HashSet<ClassHasChild>();
            Schedules = new HashSet<Schedule>();
            AlbumAlbums = new HashSet<Album>();
            Teachers = new HashSet<Teacher>();
        }

        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public int? Number { get; set; }
        public sbyte? IsActive { get; set; }
        public int SchoolId { get; set; }
        public int SemesterId { get; set; }
        public int GradeId { get; set; }
        public int? Status { get; set; }

        public virtual Grade Grade { get; set; } = null!;
        public virtual School School { get; set; } = null!;
        public virtual Semester Semester { get; set; } = null!;
        public virtual ICollection<Album> Albums { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
        public virtual ICollection<ClassHasChild> ClassHasChildren { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }

        public virtual ICollection<Album> AlbumAlbums { get; set; }
        public virtual ICollection<Teacher> Teachers { get; set; }
    }
}
