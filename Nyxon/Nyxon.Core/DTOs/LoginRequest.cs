using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class LoginRequest
    {
        [Required]
        [Length(5,20)]
        [NotNull]
        public string Username { get; set; } = string.Empty;
        [Required]
        [Length(12,30)]
        [NotNull]
        public string PasswordHash { get; set; } = string.Empty; // already hashed from client
    }
}