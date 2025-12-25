using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.Interfaces.Crypto;

namespace Nyxon.Server.Services.Auth
{
    public class PasswordService : IPasswordService
    {
        private readonly IArgon2Crypto _argon2Crypto;

        public PasswordService(IArgon2Crypto argon2Crypto)
        {
            _argon2Crypto = argon2Crypto;
        }

        public byte[] HashPassword(byte[] password, byte[] salt)
        {
            return _argon2Crypto.HashPassword(password, salt, 32);
        }

        public bool VerifyPassword(byte[] password, byte[] salt, byte[] expectedHash)
        {
            return HashPassword(password, salt).SequenceEqual(expectedHash);
        }
    }
}