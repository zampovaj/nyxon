using Xunit;
using Nyxon.Core.Crypto;
using System;
using Nyxon.Core.Services.Keys;
using Nyxon.Core.Interfaces;

namespace Nyxon.Core.Tests.Crypto
{
    public class SymmetricKeyServiceTests
    {
        private readonly IRandomService _randomService = new RandomService();
        private readonly IArgon2Crypto _argon2Crypto = new Argon2Crypto();
        private readonly ISymmetricKeyService _service;

        public SymmetricKeyServiceTests()
        {
            _service = new SymmetricKeyService(_argon2Crypto, _randomService);
        }

        [Fact]
        public void GenerateVaultKey_Returns32Bytes()
        {
            var key = _service.GenerateVaultKey();
            Assert.NotNull(key);
            Assert.Equal(32, key.Length);
        }

        [Fact]
        public void DeriveKeyFromPassphrase_ReturnsSameKeyForSameInput()
        {
            var salt = _randomService.GenerateRandomBytes(16);
            var key1 = _service.DeriveKeyFromPassphrase("password123!", salt);
            var key2 = _service.DeriveKeyFromPassphrase("password123!", salt);

            Assert.Equal(key1, key2);
        }
    }
}
