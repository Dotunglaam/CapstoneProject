using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Token
    {
        public int TokenId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
