using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Album
    {
        public Album()
        {
            Images = new HashSet<Image>();
            ClassClasses = new HashSet<Class>();
        }

        public int AlbumId { get; set; }
        public int ClassId { get; set; }
        public int CreateBy { get; set; }
        public int ModifiBy { get; set; }
        public string? AlbumName { get; set; }
        public DateTime? TimePost { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }
        public int? IsActive { get; set; }
        public string? Reason { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual User CreateByNavigation { get; set; } = null!;
        public virtual User ModifiByNavigation { get; set; } = null!;
        public virtual ICollection<Image> Images { get; set; }

        public virtual ICollection<Class> ClassClasses { get; set; }
    }
}
