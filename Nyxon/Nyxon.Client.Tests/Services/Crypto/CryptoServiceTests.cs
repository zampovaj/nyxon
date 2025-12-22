using System;
using System.Linq;
using Xunit;
using Nyxon.Client.Services.Crypto;
using Nyxon.Core.Crypto;
using Nyxon.Core.Services.Keys;
using Nyxon.Core.Models;
using Nyxon.Core.Interfaces;

namespace Nyxon.Client.Tests.Services.Crypto
{
    public class CryptoServiceTests
    {
        private readonly ICryptoService _cryptoService;
        private readonly IKeyGenerationService _keyGenService;

        public CryptoServiceTests()
        {
            // 1. Setup the Core dependencies (Matching your KeyGenerationServiceTests)
            var randomService = new RandomService();
            var aesCrypto = new AesCrypto(randomService);
            var argon2 = new Argon2Crypto();
            var symmetricKeyService = new SymmetricKeyService(argon2, randomService);
            var x25519 = new X25519Crypto();
            var ed25519 = new Ed25519Crypto();

            // 2. Setup the Services
            _keyGenService = new KeyGenerationService(symmetricKeyService, randomService, x25519, ed25519, aesCrypto);
            _cryptoService = new CryptoService(_keyGenService);
        }

        [Fact]
        public void GeneratePrekeyBundle_EncryptsPrivateKeys_AndCreatesCorrectCount()
        {
            // Arrange
            var identityKey = _cryptoService.GenerateIdentityKey(); // Returns AsymmetricKey
            var vaultKey = _cryptoService.GenerateVaultKey();       // Returns byte[32]
            int opkCount = 5;

            // Act
            PrekeyBundle bundle = _cryptoService.GeneratePrekeyBundle(identityKey.PrivateKey, vaultKey, opkCount);

            // Assert
            Assert.NotNull(bundle);
            Assert.NotNull(bundle.SPK);
            Assert.Equal(opkCount, bundle.OPKs.Count);

            // -- CRITICAL SECURITY CHECK --
            // The Private Keys in the bundle MUST be encrypted.
            // A raw X25519 private key is exactly 32 bytes.
            // An encrypted key (AES-GCM) will be 32 bytes + Nonce(12) + Tag(16) = ~60 bytes.
            
            // 1. Verify SPK Private Key is encrypted (length check)
            Assert.True(bundle.SPK.PrivateKey.Length > 32, 
                "SPK Private Key should be larger than 32 bytes (indicating encryption overhead).");

            // 2. Verify we can Decrypt it back to 32 bytes using the vault key
            byte[] decryptedSpkPriv = _cryptoService.DecryptKey(bundle.SPK.PrivateKey, vaultKey);
            Assert.Equal(32, decryptedSpkPriv.Length);

            // 3. Verify OPK Private Keys are encrypted
            foreach (var opk in bundle.OPKs)
            {
                Assert.True(opk.PrivateKey.Length > 32, 
                    $"OPK Id {opk.Id} Private Key should be encrypted.");
                
                byte[] decryptedOpkPriv = _cryptoService.DecryptKey(opk.PrivateKey, vaultKey);
                Assert.Equal(32, decryptedOpkPriv.Length);
            }
        }

        [Fact]
        public void GeneratePrekeyBundle_WithZeroOPKs_ReturnsEmptyList()
        {
            // Arrange
            var identityKey = _cryptoService.GenerateIdentityKey();
            var vaultKey = _cryptoService.GenerateVaultKey();

            // Act
            var bundle = _cryptoService.GeneratePrekeyBundle(identityKey.PrivateKey, vaultKey, 0);

            // Assert
            Assert.NotNull(bundle.SPK);
            Assert.Empty(bundle.OPKs);
        }

        [Fact]
        public void EncryptDecrypt_RoundTrip_Works()
        {
            // Arrange
            var key = _cryptoService.GenerateVaultKey();
            var data = System.Text.Encoding.UTF8.GetBytes("Super Secret Data");

            // Act
            var ciphertext = _cryptoService.EncryptKey(data, key);
            var plaintext = _cryptoService.DecryptKey(ciphertext, key);

            // Assert
            Assert.NotEqual(data, ciphertext); // Ciphertext should look different
            Assert.Equal(data, plaintext);     // Decryption should restore data
        }

        [Fact]
        public void DerivePassphraseKey_DelegatesIdeally()
        {
            // Arrange
            string pass = "password123";
            byte[] salt = _cryptoService.GeneratePassphraseSalt();

            // Act
            byte[] key1 = _cryptoService.DerivePassphraseKey(pass, salt);
            byte[] key2 = _cryptoService.DerivePassphraseKey(pass, salt);

            // Assert
            Assert.NotNull(key1);
            Assert.Equal(key1, key2); // Deterministic
            Assert.Equal(32, key1.Length); // Assuming Argon2 default is 32 bytes
        }
    }
}