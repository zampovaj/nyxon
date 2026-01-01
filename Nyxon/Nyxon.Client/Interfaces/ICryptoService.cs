using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSec.Cryptography;

namespace Nyxon.Client.Interfaces.Crypto
{
    public interface ICryptoService
    {
        byte[] GeneratePassphraseSalt();
        byte[] GeneratePasswordSalt();
        Task<byte[]> DerivePassphraseKeyAsync(byte[] passphrase, byte[] salt);
        byte[] GenerateVaultKey();
        PrekeyBundle GeneratePrekeyBundle(byte[] privateIdentityKey, byte[] vaultKey, int opkCount);
        AsymmetricKey GenerateIdentityKey();
        byte[] EncryptWithKey(byte[] data, byte[] key, byte[]? aad = null);
        byte[] DecryptWithKey(byte[] data, byte[] key, byte[]? aad = null);
        AsymmetricKey GenerateEphemeralKeyPair();
        byte[] DeriveSharedSecret(byte[] privateKey, byte[] publicKey);
        (byte[] ChainKey1, byte[] ChainKey2) SplitRootKey(byte[] rootKey, string label1, string label2);
        Task<bool> VerifySignatureAsync(byte[] data, byte[] signature, byte[] publicKey);
    }
}