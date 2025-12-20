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

        public KeyGenerationService(ISymmetricKeyService symmetricKeyService,
            IRandomService random,
            IX25519Crypto x25519,
            IEd25519Crypto ed25519)
        {
            _symmetricKeyService = symmetricKeyService;
            _random = random;
            _x25519 = x25519;
            _ed25519 = ed25519;
        }

        public byte[] DeriveKeyFromPassphrase(string passphrase, byte[] salt)
        {
            return _symmetricKeyService.DeriveKeyFromPassphrase(passphrase, salt);
        }

        public CryptographicKey GenerateEphemeralKeyPair()
        {
            (byte[] publicBytes, byte[] privateBytes) = _x25519.GenerateKeyPair();
            return new CryptographicKey(publicBytes, privateBytes);
        }

        public CryptographicKey GenerateIdentityKeyPair()
        {
            (byte[] publicBytes, byte[] privateBytes) = _ed25519.GenerateKeyPair();
            return new CryptographicKey(publicBytes, privateBytes);
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
            throw new NotImplementedException();
        }

        public byte[] SignWithIdentityKey(byte[] data, byte[] privateKey)
        {
            throw new NotImplementedException();
        }

        public bool VerifyWithIdentityKey(byte[] data, byte[] signature, byte[] publicKey)
        {
            throw new NotImplementedException();
        }

        List<CryptographicKey> IKeyGenerationService.GeneratePrekeyBundle(int count)
        {
            throw new NotImplementedException();
        }
    }
}