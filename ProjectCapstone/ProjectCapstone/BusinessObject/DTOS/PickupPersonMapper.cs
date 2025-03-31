using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class PickupPersonInfoDto
    {
        public int PickupPersonID { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string UUID { get; set; }
        public string ImageUrl { get; set; }
        public List<StudentInfoDto> Students { get; set; }
    }

    public class StudentInfoDto
    {
        public int StudentID { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public string Avatar { get; set; }
    }

    public class AddPickupPersonDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Uuid { get; set; }
        public string ImageUrl { get; set; }
        public int ParentId { get; set; } 
        public List<int> StudentIds { get; set; }
    }


}
