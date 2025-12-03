using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}