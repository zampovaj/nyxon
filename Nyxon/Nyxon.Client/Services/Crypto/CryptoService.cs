using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces.Crypto;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using Nyxon.Core.Services;

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

        public AsymmetricKey GenerateAgreementKey()
        {
            return _keyGenerationService.GenerateEphemeralKeyPair();
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

        public PrekeyBundle GeneratePrekeyBundle(byte[] privateIdentityKey, byte[] vaultKey, int opkCount, Guid userId)
        {
            List<OneTimePrekey?> opkList = null;
            SignedPrekey? spk = null;

            try
            {
                opkList = new List<OneTimePrekey>();
                spk = _keyGenerationService.GenerateSPK(privateIdentityKey);
                spk.PrivateKey = _keyGenerationService.EncryptWithKey(spk.PrivateKey, vaultKey, AadFactory.ForSpk(userId));

                for (int i = 0; i < opkCount; i++)
                {
                    var opk = _keyGenerationService.GenerateOPK();
                    opk.PrivateKey = _keyGenerationService.EncryptWithKey(opk.PrivateKey, vaultKey, AadFactory.ForOpk(userId));

                    if (opk == null)
                        throw new ArgumentNullException(nameof(opk), "OneTimePrekey generation failed.");

                    opkList.Add(opk);
                }

                return new PrekeyBundle(spk, opkList);
            }
            finally
            {
                foreach (var opk in opkList)
                {
                    if (opk != null && opk.PrivateKey != null)
                    {
                        CryptographicOperations.ZeroMemory(opk.PrivateKey);
                    }
                }
                if (spk != null && spk.PrivateKey != null) CryptographicOperations.ZeroMemory(spk.PrivateKey);
            }
        }

        public SignedPrekey GenerateSpk(byte[] privateIdentityKey, byte[] vaultKey, Guid userId)
        {
            SignedPrekey? spk = null;
            try
            {
                spk = _keyGenerationService.GenerateSPK(privateIdentityKey);
                spk.PrivateKey = _keyGenerationService.EncryptWithKey(spk.PrivateKey, vaultKey, AadFactory.ForSpk(userId));

                return spk;
            }
            finally
            {
                if (spk != null && spk.PrivateKey != null) CryptographicOperations.ZeroMemory(spk.PrivateKey);
            }
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

        public byte[] AdvanceRatchet(byte[] key, int rotationIndex, Guid conversationId)
        {
            byte[] salt = Encoding.UTF8.GetBytes($"{conversationId}:{rotationIndex}");
            byte[] info = Encoding.UTF8.GetBytes($"ratchet:{conversationId}:{rotationIndex}:v1");
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, key, 32, salt, info);
        }
        public byte[] SignData(byte[] data, byte[] privateKey)
        {
            return _keyGenerationService.SignWithIdentityKey(data, privateKey);
        }

        public byte[] DeriveMessageKey(byte[] key, int rotationIndex, int messageIndex, Guid conversationId)
        {
            byte[] info = Encoding.UTF8.GetBytes($"msg:{conversationId}:{rotationIndex}:{messageIndex}:v1");
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, key, 32, null, info);
        }
    }
}