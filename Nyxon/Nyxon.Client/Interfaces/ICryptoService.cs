using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSec.Cryptography;

namespace Nyxon.Client.Interfaces
{
    public interface ICryptoService
    {
        byte[] GeneratePassphraseSalt();
        byte[] GeneratePasswordSalt();
        byte[] DerivePassphraseKey(string passphrase, byte[] salt);
        byte[] GenerateVaultKey();
        PrekeyBundle GeneratePrekeyBundle(byte[] privateIdentityKey, byte[] vaultKey, int opkCount);
        AsymmetricKey GenerateIdentityKey();
        byte[] EncryptKey(byte[] data, byte[] key);
        byte[] DecryptKey(byte[] data, byte[] key);

    }
}