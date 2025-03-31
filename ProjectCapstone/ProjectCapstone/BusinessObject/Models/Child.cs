using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Child
    {
        public Child()
        {
            AttendanceDetails = new HashSet<AttendanceDetail>();
            Checkservices = new HashSet<Checkservice>();
            ChildrenHasServices = new HashSet<ChildrenHasService>();
            ClassHasChildren = new HashSet<ClassHasChild>();
            Payments = new HashSet<Payment>();
            Tuitions = new HashSet<Tuition>();
            PickupPeople = new HashSet<PickupPerson>();
        }

        public int StudentId { get; set; }
        public int ParentId { get; set; }
        public int? GradeId { get; set; }
        public string Code { get; set; } = null!;
        public string? FullName { get; set; }
        public string? NickName { get; set; }
        public DateTime? Dob { get; set; }
        public sbyte? Gender { get; set; }
        public sbyte? Status { get; set; }
        public string? EthnicGroups { get; set; }
        public string? Nationality { get; set; }
        public string? Religion { get; set; }
        public string? Avatar { get; set; }

        public virtual Grade? Grade { get; set; }
        public virtual Parent Parent { get; set; } = null!;
        public virtual ICollection<AttendanceDetail> AttendanceDetails { get; set; }
        public virtual ICollection<Checkservice> Checkservices { get; set; }
        public virtual ICollection<ChildrenHasService> ChildrenHasServices { get; set; }
        public virtual ICollection<ClassHasChild> ClassHasChildren { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Tuition> Tuitions { get; set; }

        public virtual ICollection<PickupPerson> PickupPeople { get; set; }
    }
}
