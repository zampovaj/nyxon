using System;
using Xunit;
using Nyxon.Core.Crypto;
using System.Security.Cryptography;

namespace Nyxon.Core.Tests
{
    public class Argon2CryptoTest
    {
        [Fact]
        public void DeriveKey_SamePassphraseAndSalt_ReturnsSameKey()
        {
            // Arrange
            var crypto = new Argon2Crypto();
            string passphrase = "MySuperSecurePassphrase!";
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            // Act
            byte[] key1 = crypto.DeriveKey(passphrase, salt, 32);
            byte[] key2 = crypto.DeriveKey(passphrase, salt, 32);

            // Assert
            Assert.Equal(key1, key2);
        }

        [Fact]
        public void DeriveKey_DifferentSalt_ReturnsDifferentKey()
        {
            // Arrange
            var crypto = new Argon2Crypto();
            string passphrase = "MySuperSecurePassphrase!";
            byte[] salt1 = new byte[16];
            byte[] salt2 = new byte[16];
            RandomNumberGenerator.Fill(salt1);
            RandomNumberGenerator.Fill(salt2);

            // Act
            byte[] key1 = crypto.DeriveKey(passphrase, salt1, 32);
            byte[] key2 = crypto.DeriveKey(passphrase, salt2, 32);

            // Assert
            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void DeriveKey_DifferentLength_ReturnsCorrectLength()
        {
            // Arrange
            var crypto = new Argon2Crypto();
            string passphrase = "MySuperSecurePassphrase!";
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            // Act
            byte[] key = crypto.DeriveKey(passphrase, salt, 64);

            // Assert
            Assert.Equal(64, key.Length);
        }
    }
}
