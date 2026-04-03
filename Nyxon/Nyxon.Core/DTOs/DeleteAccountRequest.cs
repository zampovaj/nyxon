using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class DeleteAccountRequest
    {
        [Required, NotNull]
        public byte[] PasswordHash { get; set; } // already prehashed from client
    }
}