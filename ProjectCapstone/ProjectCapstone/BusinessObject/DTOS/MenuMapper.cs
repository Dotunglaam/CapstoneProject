using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class MenuHasGradeMapper
    {
        public int MenuID { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Status { get; set; }
        public List<int> GradeIDs { get; set; } = new List<int>();
        public List<MenuDetailMapper> MenuDetails { get; set; }
    }
    public class MenuMapper
    {
        public int MenuID { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Status { get; set; }
        public List<MenuDetailMapper> MenuDetails { get; set; }
    }

    public class MenuStatusMapper
    {
        public int MenuID { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Status { get; set; }
    }
}
