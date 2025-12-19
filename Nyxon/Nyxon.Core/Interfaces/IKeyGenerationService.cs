using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces
{
    public interface IKeyGenerationService
    {
        byte[] GenerateVaultKey();
        byte[] DeriveKeyFromPassphrase(string passphrase, byte[] salt);

        (byte[] PublicKey, byte[] PrivateKey) GenerateIdentityKeyPair();
        byte[] SignWithIdentityKey(byte[] data, byte[] privateKey);
        bool VerifyWithIdentityKey(byte[] data, byte[] signature, byte[] publicKey);

        (byte[] PublicKey, byte[] PrivateKey) GenerateEphemeralKeyPair();

        List<(byte[] PublicKey, byte[] PrivateKey)> GeneratePrekeyBundle(int count);

        byte[] GenerateRandomBytes(int length);
        byte[] GenerateRandomSalt(int length);
    }

}