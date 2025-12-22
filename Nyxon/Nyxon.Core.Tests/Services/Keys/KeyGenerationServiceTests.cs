using System;
using System.Linq;
using Nyxon.Core.Models.Vaults;
using Nyxon.Core.Services.Keys;
using Nyxon.Core.Crypto;
using Xunit;

namespace Nyxon.Core.Tests.Services.Keys
{
    public class KeyGenerationServiceTests
    {
        private readonly KeyGenerationService _service;

        public KeyGenerationServiceTests()
        {
            var randomService = new RandomService(); // IRandomService implementation
            var aesCrypto = new AesCrypto(randomService);
            var argon2 = new Argon2Crypto();
            var symmetricKeyService = new SymmetricKeyService(argon2, randomService); // implement as needed
            var x25519 = new X25519Crypto();
            var ed25519 = new Ed25519Crypto();

            _service = new KeyGenerationService(symmetricKeyService, randomService, x25519, ed25519, aesCrypto);
        }

        [Fact]
        public void GenerateEphemeralKeyPair_ReturnsNonNullKeys()
        {
            var key = _service.GenerateEphemeralKeyPair();
            Assert.NotNull(key.PublicKey);
            Assert.NotNull(key.PrivateKey);
        }

        [Fact]
        public void GenerateIdentityKeyPair_ReturnsNonNullKeys()
        {
            var key = _service.GenerateIdentityKeyPair();
            Assert.NotNull(key.PublicKey);
            Assert.NotNull(key.PrivateKey);
        }

        [Fact]
        public void SignAndVerify_WorksCorrectly()
        {
            var key = _service.GenerateIdentityKeyPair();
            var data = System.Text.Encoding.UTF8.GetBytes("Hello world");

            var signature = _service.SignWithIdentityKey(data, key.PrivateKey);
            bool verified = _service.VerifyWithIdentityKey(data, signature, key.PublicKey);

            Assert.True(verified);
        }

        [Fact]
        public void EncryptDecrypt_ReturnsOriginalData()
        {
            var key = _service.GenerateRandomBytes(32); // AES-256 key
            var plaintext = System.Text.Encoding.UTF8.GetBytes("Secret message");

            var ciphertext = _service.EncryptWithKey(plaintext, key);
            var decrypted = _service.DecryptWithKey(ciphertext, key);

            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void GenerateRandomSalt_ReturnsCorrectLength()
        {
            var salt = _service.GenerateRandomSalt(16);
            Assert.Equal(16, salt.Length);
        }

        [Fact]
        public void GenerateVaultKey_ReturnsNonNull()
        {
            var vaultKey = _service.GenerateVaultKey();
            Assert.NotNull(vaultKey);
            Assert.True(vaultKey.Length > 0);
        }

        [Fact]
        public void GenerateOPK_ReturnsNonNullKeys()
        {
            var opk = _service.GenerateOPK();
            Assert.NotNull(opk.PublicKey);
            Assert.NotNull(opk.PrivateKey);
        }

        [Fact]
        public void GenerateSPK_SignatureVerifiable()
        {
            var identityKey = _service.GenerateIdentityKeyPair();
            var spk = _service.GenerateSPK(identityKey.PrivateKey);

            Assert.NotNull(spk.PublicKey);
            Assert.NotNull(spk.PrivateKey);
            Assert.NotNull(spk.Signature);

            // Verify the SPK signature using the identity key
            bool verified = _service.VerifyWithIdentityKey(spk.PublicKey, spk.Signature, identityKey.PublicKey);
            Assert.True(verified);
        }
    }
}
