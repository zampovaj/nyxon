using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string InviteCode {get; set;}

        // keys created on client
        public byte[] PublicKey {get; set;} = Array.Empty<byte>();
        public byte[] EncryptedVaultKey {get; set;} = Array.Empty<byte>();
        public byte[] EncryptedIdentityKey {get; set;} = Array.Empty<byte>();

    }
}