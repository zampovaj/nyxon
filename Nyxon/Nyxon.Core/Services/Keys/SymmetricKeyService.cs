using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Services.Keys
{
    public class SymmetricKeyService : ISymmetricKeyService
    {
        private readonly IArgon2Crypto _argon2Crypto;
        private readonly IRandomService _randomService;

        public SymmetricKeyService(IArgon2Crypto argon2Crypto, IRandomService randomService)
        {
            _argon2Crypto = argon2Crypto;
            _randomService = randomService;
        }

        public byte[] DeriveKeyFromPassphrase(byte[] passphrase, byte[] salt, int length = 32)
        {
            return _argon2Crypto.DeriveKey(passphrase, salt, length);
        }

        public byte[] GenerateVaultKey()
        {
            //32 cause aes-256 uses 32 bytes
            return _randomService.GenerateRandomBytes(32);
        }
    }
}