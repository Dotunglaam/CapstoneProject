using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class MenuDetailMapper
    {
        public int MenuDetailId { get; set; }
        public string MealCode { get; set; }
        public string DayOfWeek { get; set; }
        public string FoodName { get; set; }

    }
}
