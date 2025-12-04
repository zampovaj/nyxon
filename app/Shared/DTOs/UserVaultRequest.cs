using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class UserVaultRequest
    {
        public byte[] EncryptedVaultKey { get; set; }
        public byte[] EncryptedIdentityKey { get; set; }
    }
}