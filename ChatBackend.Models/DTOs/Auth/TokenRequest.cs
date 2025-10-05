using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBackend.Models.DTOs.Auth
{
    public class TokenRequest
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}
