using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Nyxon.Server.Services.Crypto.Hash
{
    public class Hasher : IHasher
    {
        private string _serverSecret;
        private byte[] _secretBytes;

        public Hasher(string serverSecret)
        {
            _serverSecret = serverSecret;
            _secretBytes = Convert.FromBase64String(serverSecret);
        }

        public string HashInvite(string code)
        {
            using var hmac = new HMACSHA256(_secretBytes);
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hashBytes);
        }
    }
}