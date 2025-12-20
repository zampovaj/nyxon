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
        byte[] DeriveKeyFromPassphrase(string passphrase, byte[] salt);

        CryptographicKey GenerateIdentityKeyPair();
        byte[] SignWithIdentityKey(byte[] data, byte[] privateKey);
        bool VerifyWithIdentityKey(byte[] data, byte[] signature, byte[] publicKey);

        CryptographicKey GenerateEphemeralKeyPair();

        List<CryptographicKey> GeneratePrekeyBundle(int count);

        byte[] GenerateRandomBytes(int length);
        byte[] GenerateRandomSalt(int length);
    }

}