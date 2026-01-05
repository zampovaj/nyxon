using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces.Crypto;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;

namespace Nyxon.Client.Services.Crypto
{
    public class CryptoService : ICryptoService
    {
        private readonly IKeyGenerationService _keyGenerationService;
        private readonly IArgon2Crypto _argon2Crypto;

        public CryptoService(IKeyGenerationService keyGenerationService, IArgon2Crypto argon2Crypto)
        {
            _keyGenerationService = keyGenerationService;
            _argon2Crypto = argon2Crypto;
        }

        public async Task<byte[]> DerivePassphraseKeyAsync(byte[] passphrase, byte[] salt)
        {
            return await _argon2Crypto.DerivePassphraseKeyAsync(passphrase, salt);
        }

        public byte[] EncryptWithKey(byte[] data, byte[] key, byte[]? aad = null)
        {
            return _keyGenerationService.EncryptWithKey(data, key, aad);
        }

        public byte[] DecryptWithKey(byte[] data, byte[] key, byte[]? aad = null)
        {
            return _keyGenerationService.DecryptWithKey(data, key, aad);
        }

        public AsymmetricKey GenerateIdentityKey()
        {
            return _keyGenerationService.GenerateIdentityKeyPair();
        }

        public byte[] GeneratePassphraseSalt()
        {
            return _keyGenerationService.GenerateRandomSalt(32);
        }

        public byte[] GeneratePasswordSalt()
        {
            return _keyGenerationService.GenerateRandomSalt(16);
        }

        public byte[] GenerateVaultKey()
        {
            return _keyGenerationService.GenerateVaultKey();
        }

        public PrekeyBundle GeneratePrekeyBundle(byte[] privateIdentityKey, byte[] vaultKey, int opkCount)
        {
            List<OneTimePrekey> opkList = new List<OneTimePrekey>();

            var spk = _keyGenerationService.GenerateSPK(privateIdentityKey);
            spk.PrivateKey = _keyGenerationService.EncryptWithKey(spk.PrivateKey, vaultKey);

            for (int i = 0; i < opkCount; i++)
            {
                var opk = _keyGenerationService.GenerateOPK();
                opk.PrivateKey = _keyGenerationService.EncryptWithKey(opk.PrivateKey, vaultKey);

                if (opk == null)
                    throw new ArgumentNullException(nameof(opk), "OneTimePrekey generation failed.");

                opkList.Add(opk);
            }

            return new PrekeyBundle(spk, opkList);
        }

        public AsymmetricKey GenerateEphemeralKeyPair()
        {
            return _keyGenerationService.GenerateEphemeralKeyPair();
        }

        public byte[] DeriveSharedSecret(byte[] privateKey, byte[] publicKey)
        {
            return _keyGenerationService.DeriveSharedX25519Secret(privateKey, publicKey);
        }

        public (byte[] ChainKey1, byte[] ChainKey2) SplitRootKey(byte[] rootKey, string label1, string label2)
        {
            byte[] salt = new byte[32];
            int length = 32;

            byte[] chainKey1 = new byte[length];
            byte[] chainKey2 = new byte[length];

            byte[] info1 = Encoding.UTF8.GetBytes(label1);
            byte[] info2 = Encoding.UTF8.GetBytes(label2);

            chainKey1 = HKDF.DeriveKey(HashAlgorithmName.SHA256, rootKey, length, salt, info1);
            chainKey2 = HKDF.DeriveKey(HashAlgorithmName.SHA256, rootKey, length, salt, info2);

            return (chainKey1, chainKey2);
        }
        public async Task<bool> VerifySignatureAsync(byte[] data, byte[] signature, byte[] publicKey)
        {
            return _keyGenerationService.VerifyWithIdentityKey(data, signature, publicKey);
        }

        public byte[] AdvanceRatchet(byte[] key, int RotationIndex, Guid ConversationId)
        {
            byte[] salt = Encoding.UTF8.GetBytes($"{ConversationId}:{RotationIndex}");
            byte[] info = Encoding.UTF8.GetBytes($"ratchet:{ConversationId}:{RotationIndex}:v1");
            return HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                key,
                32,
                salt,
                info
            );
        }
        public byte[] DeriveMessageKey(byte[] key, int SequenceNumber, Guid ConversationId)
        {
            byte[] info = Encoding.UTF8.GetBytes($"msg:{ConversationId}:{SequenceNumber}:v1");
            return HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                key,
                32,
                null,
                info
            );
        }
    }
}