using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Discount
    {
        public Discount()
        {
            Tuitions = new HashSet<Tuition>();
        }

        public int DiscountId { get; set; }
        public int? Number { get; set; }
        public int? Discount1 { get; set; }

        public virtual ICollection<Tuition> Tuitions { get; set; }
    }
}
