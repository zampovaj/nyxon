using System;
using Xunit;
using Nyxon.Core.Crypto;

namespace Nyxon.Core.Tests.Crypto
{
    public class Ed25519CryptoTests
    {
        private readonly IEd25519Crypto _crypto;

        public Ed25519CryptoTests()
        {
            _crypto = new Ed25519Crypto();
        }

        [Fact]
        public void GenerateKeyPair_ProducesCorrectSizes()
        {
            var (pub, priv) = _crypto.GenerateKeyPair();

            Assert.NotNull(pub);
            Assert.NotNull(priv);
            Assert.Equal(32, pub.Length);
            Assert.Equal(64, priv.Length); // IMPORTANT: Geralt private keys are 64 bytes
        }

        [Fact]
        public void SignAndVerify_ValidSignature_ReturnsTrue()
        {
            // Arrange
            var (pub, priv) = _crypto.GenerateKeyPair();
            var message = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

            // Act
            var signature = _crypto.Sign(message, priv);
            var isValid = _crypto.Verify(message, signature, pub);

            // Assert
            Assert.True(isValid);
            Assert.Equal(64, signature.Length);
        }

        [Fact]
        public void Verify_TamperedMessage_ReturnsFalse()
        {
            // Arrange
            var (pub, priv) = _crypto.GenerateKeyPair();
            var message = new byte[] { 0x1, 0x2, 0x3 };
            var signature = _crypto.Sign(message, priv);

            // Act - Modify message
            var tamperedMessage = new byte[] { 0x1, 0x2, 0x9 }; 
            var isValid = _crypto.Verify(tamperedMessage, signature, pub);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Verify_TamperedSignature_ReturnsFalse()
        {
            // Arrange
            var (pub, priv) = _crypto.GenerateKeyPair();
            var message = new byte[] { 0x1, 0x2, 0x3 };
            var signature = _crypto.Sign(message, priv);

            // Act - Flip a bit in the signature
            signature[0] ^= 0xFF; 
            var isValid = _crypto.Verify(message, signature, pub);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Verify_WrongPublicKey_ReturnsFalse()
        {
            // Arrange
            var (pubA, privA) = _crypto.GenerateKeyPair();
            var (pubB, privB) = _crypto.GenerateKeyPair();
            var message = new byte[] { 0x1, 0x2, 0x3 };

            // Act - Sign with A, Verify with B
            var signature = _crypto.Sign(message, privA);
            var isValid = _crypto.Verify(message, signature, pubB);

            // Assert
            Assert.False(isValid);
        }
    }
}