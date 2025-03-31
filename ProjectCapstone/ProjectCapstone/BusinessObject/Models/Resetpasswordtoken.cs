using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Resetpasswordtoken
    {
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiryTime { get; set; }
    }
}
