using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class UserVaultResponse
    {
        [Required]
        [NotNull]
        public byte[] EncryptedVaultKey { get; set; }
        [Required]
        [NotNull]
        public byte[] EncryptedPrivateIdentityKey { get; set; }
        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PassphraseSalt { get; set; }
    }
}