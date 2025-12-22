using System;
using Nyxon.Core.Crypto;
using Xunit;
using System.Security.Cryptography;

namespace Nyxon.Core.Tests.Crypto
{
    public class AesCryptoTests
    {
        private readonly AesCrypto _aes;

        public AesCryptoTests()
        {
            _aes = new AesCrypto(new RandomService()); // IRandomService implementation
        }

        [Fact]
        public void EncryptDecrypt_ReturnsOriginalPlaintext()
        {
            // Arrange
            byte[] key = new byte[32]; // AES-256
            new Random().NextBytes(key);
            byte[] plaintext = System.Text.Encoding.UTF8.GetBytes("Test message");

            // Act
            byte[] ciphertext = _aes.Encrypt(plaintext, key);
            byte[] decrypted = _aes.Decrypt(ciphertext, key);

            // Assert
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Encrypt_GeneratesDifferentCiphertextsForSamePlaintext()
        {
            // Arrange
            byte[] key = new byte[32];
            new Random().NextBytes(key);
            byte[] plaintext = System.Text.Encoding.UTF8.GetBytes("Same message");

            // Act
            byte[] ciphertext1 = _aes.Encrypt(plaintext, key);
            byte[] ciphertext2 = _aes.Encrypt(plaintext, key);

            // Assert
            Assert.NotEqual(ciphertext1, ciphertext2); // Different nonce → different ciphertext
        }

        [Fact]
        public void Decrypt_WithTamperedCiphertext_ThrowsCryptographicException()
        {
            // Arrange
            byte[] key = new byte[32];
            new Random().NextBytes(key);
            byte[] plaintext = System.Text.Encoding.UTF8.GetBytes("Sensitive data");

            byte[] ciphertext = _aes.Encrypt(plaintext, key);

            // Tamper with one byte in ciphertext
            ciphertext[ciphertext.Length - 1] ^= 0xFF;

            // Act & Assert
            Assert.ThrowsAny<CryptographicException>(() => _aes.Decrypt(ciphertext, key));
        }

        [Fact]
        public void Decrypt_WithWrongKey_ThrowsCryptographicException()
        {
            // Arrange
            byte[] key1 = new byte[32];
            byte[] key2 = new byte[32];
            new Random().NextBytes(key1);
            new Random().NextBytes(key2);
            byte[] plaintext = System.Text.Encoding.UTF8.GetBytes("Secret message");

            byte[] ciphertext = _aes.Encrypt(plaintext, key1);

            // Act & Assert
            Assert.ThrowsAny<CryptographicException>(() => _aes.Decrypt(ciphertext, key2));
        }

        [Fact]
        public void Encrypt_OutputHasCorrectLength()
        {
            // Arrange
            byte[] key = new byte[32];
            new Random().NextBytes(key);
            byte[] plaintext = new byte[50];

            // Act
            byte[] ciphertext = _aes.Encrypt(plaintext, key);

            // Assert
            int expectedLength = 12 /*nonce*/ + 16 /*tag*/ + plaintext.Length;
            Assert.Equal(expectedLength, ciphertext.Length);
        }
    }
}
