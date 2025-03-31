using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class PickupPerson
    {
        public PickupPerson()
        {
            Students = new HashSet<Child>();
        }

        public int PickupPersonId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Uuid { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public virtual Parent Parent { get; set; } = null!;

        public virtual ICollection<Child> Students { get; set; }
    }
}
