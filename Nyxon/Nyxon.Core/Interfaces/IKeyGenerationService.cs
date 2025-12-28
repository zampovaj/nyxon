using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSec.Cryptography;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.Interfaces
{
    public interface IKeyGenerationService
    {
        byte[] GenerateVaultKey();
        byte[] DeriveKeyFromPassphrase(byte[] passphrase, byte[] salt);

        AsymmetricKey GenerateIdentityKeyPair();
        byte[] SignWithIdentityKey(byte[] data, byte[] privateKey);
        bool VerifyWithIdentityKey(byte[] data, byte[] signature, byte[] publicKey);

        AsymmetricKey GenerateEphemeralKeyPair();

        OneTimePrekey GenerateOPK();
        SignedPrekey GenerateSPK(byte[] privateIdentityKey);

        byte[] GenerateRandomBytes(int length);
        byte[] GenerateRandomSalt(int length);
        byte[] EncryptWithKey(byte[] data, byte[] key);
        byte[] DecryptWithKey(byte[] data, byte[] key);
        byte[] DeriveSharedX25519Secret(byte[] privateKey, byte[] publicKey);
    }

}