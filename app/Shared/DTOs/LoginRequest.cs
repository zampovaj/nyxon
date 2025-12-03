using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // already hashed from client
    }
}