using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && DateTime.UtcNow < Expires;
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
