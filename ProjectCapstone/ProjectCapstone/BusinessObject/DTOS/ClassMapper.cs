using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{

    public class ClassMapper
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public int? Number { get; set; }
        public sbyte? IsActive { get; set; }
        public int SchoolId { get; set; }
        public int SemesterId { get; set; }
        public int GradeId { get; set; }
        public int? Status { get; set; }

    }

    public class ClassMapper2
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public int? Number { get; set; }
        public sbyte? IsActive { get; set; }
        public int SchoolId { get; set; }
        public int SemesterId { get; set; }
        public int GradeId { get; set; }
        public int? Status { get; set; }
        public List<TeacherMapper>? Teachers { get; set; }

    }
  
        public class TeacherMapper
        {   

            public int TeacherId { get; set; }
            public string? Avatar { get; set; }
            public string? TeacherName { get; set; }
            public string? Mail  { get; set; }
            public string? Code { get; set; } // Mã Code từ Use
            public sbyte Status { get; set; }
            public sbyte HomeroomTeacher { get; set; }

    }


}
