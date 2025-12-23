using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Nyxon.Core.Interfaces.Crypto;

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

        public byte[] HashInvite(string code)
        {
            using var hmac = new HMACSHA256(_secretBytes);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
        }
    }
}