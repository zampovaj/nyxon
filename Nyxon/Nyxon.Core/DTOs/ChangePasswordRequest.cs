using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class ChangePasswordRequest
    {
        [Required, NotNull]
        public byte[] CurrentPasswordHash { get; set; } // already prehashed from client

        [Required, NotNull]
        public byte[] NewPasswordHash { get; set; } // already prehashed from client

        [Required, NotNull]
        [MinLength(16), MaxLength(16)]
        public byte[] NewPasswordSalt { get; set; }
    }
}