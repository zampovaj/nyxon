using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Interfaces;
using System.Text;
using System.Security.Cryptography;

namespace Backend.Services.Crypto
{
    public class Sha256HashService : IHashInterface
    {
        public string HashInviteCode(string rawCode)
        {
            if (string.IsNullOrEmpty(rawCode)) return string.Empty;

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawCode);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}