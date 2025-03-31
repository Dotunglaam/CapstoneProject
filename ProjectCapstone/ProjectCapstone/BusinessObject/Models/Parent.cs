using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Parent
    {
        public Parent()
        {
            Children = new HashSet<Child>();
            PickupPeople = new HashSet<PickupPerson>();
        }

        public int ParentId { get; set; }
        public string? Name { get; set; }

        public virtual User ParentNavigation { get; set; } = null!;
        public virtual ICollection<Child> Children { get; set; }
        public virtual ICollection<PickupPerson> PickupPeople { get; set; }
    }
}
