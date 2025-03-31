using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Image
    {
        public int ImageId { get; set; }
        public int AlbumId { get; set; }
        public string? ImgUrl { get; set; }
        public DateTime? PostedAt { get; set; }
        public string? Caption { get; set; }

        public virtual Album Album { get; set; } = null!;
    }
}
