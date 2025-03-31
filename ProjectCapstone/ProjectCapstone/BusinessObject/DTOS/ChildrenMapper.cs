using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class ChildrenClassMapper
    {
        public int StudentId { get; set; }
        public string Code { get; set; } = null!;
        public string? FullName { get; set; }
        public string? NickName { get; set; }
        public int? GradeId { get; set; }
        public DateTime? Dob { get; set; }
        public sbyte? Gender { get; set; }
        public sbyte? Status { get; set; }
        public string? EthnicGroups { get; set; }
        public string? Nationality { get; set; }
        public string? Religion { get; set; }
        public int ParentId { get; set; }
        public string? Avatar { get; set; }
        public List<ClassMapper> Classes { get; set; } = new List<ClassMapper>();
    }

    public class ChildrenMapper
    {
        public int StudentId { get; set; }
        public string Code { get; set; } = null!;
        public string? FullName { get; set; }
        public string? NickName { get; set; }
        public int? GradeId { get; set; }
        public DateTime? Dob { get; set; }
        public sbyte? Gender { get; set; }
        public sbyte? Status { get; set; }
        public string? EthnicGroups { get; set; }
        public string? Nationality { get; set; }
        public string? Religion { get; set; }
        public int ParentId { get; set; }
        public string? Avatar { get; set; }
    }
}
