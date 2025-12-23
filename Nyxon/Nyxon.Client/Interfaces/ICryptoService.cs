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
        Task<byte[]> DerivePassphraseKeyAsync(byte[] passphrase, byte[] salt);
        byte[] GenerateVaultKey();
        PrekeyBundle GeneratePrekeyBundle(byte[] privateIdentityKey, byte[] vaultKey, int opkCount);
        AsymmetricKey GenerateIdentityKey();
        byte[] EncryptWithKey(byte[] data, byte[] key);
        byte[] DecryptKey(byte[] data, byte[] key);

    }
}