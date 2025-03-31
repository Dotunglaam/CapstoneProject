using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Menudetail
    {
        public int MenuDetailId { get; set; }
        public int MenuId { get; set; }
        public string MealCode { get; set; } = null!;
        public string DayOfWeek { get; set; } = null!;
        public string FoodName { get; set; } = null!;

        public virtual Menu Menu { get; set; } = null!;
    }
}
