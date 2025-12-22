using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class UserVaultRequest
    {
        public byte[] EncryptedVaultKey { get; set; }
        public byte[] EncryptedIdentityKey { get; set; }
        public byte[] Salt { get; set; }
    }
}