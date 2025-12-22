using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSec.Cryptography;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.Services.Keys
{
    public class KeyGenerationService : IKeyGenerationService
    {
        private readonly IRandomService _random;
        private readonly ISymmetricKeyService _symmetricKeyService;
        private readonly IX25519Crypto _x25519;
        private readonly IEd25519Crypto _ed25519;
        private readonly IAesCrypto _aes;

        public KeyGenerationService(ISymmetricKeyService symmetricKeyService,
            IRandomService random,
            IX25519Crypto x25519,
            IEd25519Crypto ed25519,
            IAesCrypto aes)
        {
            _symmetricKeyService = symmetricKeyService;
            _random = random;
            _x25519 = x25519;
            _ed25519 = ed25519;
            _aes = aes;
        }

        public byte[] DeriveKeyFromPassphrase(byte[] passphrase, byte[] salt)
        {
            return _symmetricKeyService.DeriveKeyFromPassphrase(passphrase, salt);
        }

        public AsymmetricKey GenerateEphemeralKeyPair()
        {
            (byte[] publicBytes, byte[] privateBytes) = _x25519.GenerateKeyPair();
            return new AsymmetricKey(publicBytes, privateBytes);
        }

        public AsymmetricKey GenerateIdentityKeyPair()
        {
            (byte[] publicBytes, byte[] privateBytes) = _ed25519.GenerateKeyPair();
            return new AsymmetricKey(publicBytes, privateBytes);
        }

        public byte[] GenerateRandomBytes(int length)
        {
            return _random.GenerateRandomBytes(length);
        }

        public byte[] GenerateRandomSalt(int length)
        {
            return GenerateRandomBytes(length);
        }

        public byte[] GenerateVaultKey()
        {
            return _symmetricKeyService.GenerateVaultKey();
        }

        public byte[] SignWithIdentityKey(byte[] data, byte[] privateKey)
        {
            return _ed25519.Sign(data, privateKey);
        }

        public bool VerifyWithIdentityKey(byte[] data, byte[] signature, byte[] publicKey)
        {
            return _ed25519.Verify(data, signature, publicKey);
        }

        public byte[] EncryptWithKey(byte[] data, byte[] key)
        {
            return _aes.Encrypt(data, key);
        }

        public byte[] DecryptWithKey(byte[] data, byte[] key)
        {
            return _aes.Decrypt(data, key);
        }

        public OneTimePrekey GenerateOPK()
        {
            var key = _x25519.GenerateKeyPair();
            return new OneTimePrekey(key.PublicKey, key.PrivateKey);
        }

        public SignedPrekey GenerateSPK(byte[] privateIdentityKey)
        {
            var key = _x25519.GenerateKeyPair();
            var signature = _ed25519.Sign(key.PublicKey, privateIdentityKey);
            return new SignedPrekey(key.PublicKey, key.PrivateKey, signature);
        }
    }
}