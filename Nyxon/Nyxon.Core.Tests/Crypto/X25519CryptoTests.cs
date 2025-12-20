using System;
using Xunit;
using Nyxon.Core.Crypto;

namespace Nyxon.Core.Tests.Crypto
{
    public class X25519CryptoTests
    {
        private readonly IX25519Crypto _crypto;

        public X25519CryptoTests()
        {
            _crypto = new X25519Crypto();
        }

        [Fact]
        public void GenerateKeyPair_ReturnsValidCurve25519Keys()
        {
            // Act
            var (publicKey, privateKey) = _crypto.GenerateKeyPair();

            // Assert
            Assert.NotNull(publicKey);
            Assert.NotNull(privateKey);
            
            // X25519 keys must be exactly 32 bytes
            Assert.Equal(32, publicKey.Length);
            Assert.Equal(32, privateKey.Length);

            // Sanity check: Public and Private keys should not be identical
            Assert.NotEqual(publicKey, privateKey);
            
            // Sanity check: Two successive generations should produce different keys
            var (pub2, priv2) = _crypto.GenerateKeyPair();
            Assert.NotEqual(publicKey, pub2);
        }

        [Fact]
        public void ComputeSharedSecret_IsSymmetric_AliceAndBobAgree()
        {
            // This simulates the core requirement for X3DH:
            // DH(AlicePriv, BobPub) MUST EQUAL DH(BobPriv, AlicePub)

            // Arrange
            var (alicePub, alicePriv) = _crypto.GenerateKeyPair();
            var (bobPub, bobPriv) = _crypto.GenerateKeyPair();

            // Act
            byte[] aliceShared = _crypto.DeriveSharedSecret(alicePriv, bobPub);
            byte[] bobShared = _crypto.DeriveSharedSecret(bobPriv, alicePub);

            // Assert
            Assert.Equal(aliceShared, bobShared);
            Assert.Equal(32, aliceShared.Length);
        }

        [Theory]
        [InlineData(31)] // Too short
        [InlineData(33)] // Too long
        [InlineData(0)]  // Empty
        public void ComputeSharedSecret_ThrowsOnInvalidKeyLength(int invalidLength)
        {
            // Arrange
            var validKey = new byte[32];
            var invalidKey = new byte[invalidLength];
            new Random().NextBytes(validKey);

            // Act & Assert
            // Case 1: Invalid Private Key
            Assert.Throws<ArgumentException>(() => 
                _crypto.DeriveSharedSecret(invalidKey, validKey));

            // Case 2: Invalid Public Key
            Assert.Throws<ArgumentException>(() => 
                _crypto.DeriveSharedSecret(validKey, invalidKey));
        }

        [Fact]
        public void ComputeSharedSecret_ThrowsOnNullInputs()
        {
            var validKey = new byte[32];

            Assert.Throws<ArgumentNullException>(() => 
                _crypto.DeriveSharedSecret(null, validKey));

            Assert.Throws<ArgumentNullException>(() => 
                _crypto.DeriveSharedSecret(validKey, null));
        }

        [Fact]
        public void ComputeSharedSecret_Integration_X3DH_Flow()
        {
            // This test simulates a simplified "DH1 || DH2" concatenation flow
            // to ensure the outputs are distinct and ready for KDF.
            
            // Arrange
            var (aliceIdentityPub, aliceIdentityPriv) = _crypto.GenerateKeyPair();
            var (bobSignedPreKeyPub, bobSignedPreKeyPriv) = _crypto.GenerateKeyPair();
            var (aliceEphemeralPub, aliceEphemeralPriv) = _crypto.GenerateKeyPair();

            // Act
            // DH1 = DH(IK_A, SPK_B)
            byte[] dh1 = _crypto.DeriveSharedSecret(aliceIdentityPriv, bobSignedPreKeyPub);
            
            // DH2 = DH(EK_A, SPK_B)
            byte[] dh2 = _crypto.DeriveSharedSecret(aliceEphemeralPriv, bobSignedPreKeyPub);

            // Assert
            Assert.NotEqual(dh1, dh2); // Secrets must differ
            Assert.Equal(32, dh1.Length);
            Assert.Equal(32, dh2.Length);
        }
    }
}