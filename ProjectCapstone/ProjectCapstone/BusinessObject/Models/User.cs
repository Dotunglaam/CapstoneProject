using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class User
    {
        public User()
        {
            AlbumCreateByNavigations = new HashSet<Album>();
            AlbumModifiByNavigations = new HashSet<Album>();
            Requests = new HashSet<Request>();
            Tokens = new HashSet<Token>();
            Usernotifications = new HashSet<Usernotification>();
        }

        public int UserId { get; set; }
        public string? Firstname { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string Mail { get; set; } = null!;
        public int RoleId { get; set; }
        public string Password { get; set; } = null!;
        public string SaltKey { get; set; } = null!;
        public sbyte Status { get; set; }
        public sbyte? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string? Code { get; set; }
        public string? Avatar { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual Parent? Parent { get; set; }
        public virtual Teacher? Teacher { get; set; }
        public virtual ICollection<Album> AlbumCreateByNavigations { get; set; }
        public virtual ICollection<Album> AlbumModifiByNavigations { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<Token> Tokens { get; set; }
        public virtual ICollection<Usernotification> Usernotifications { get; set; }
    }
}
