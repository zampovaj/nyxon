using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Services.Vault
{
    public class MockDecryptionService : IVaultDecryptionService
    {
        public async Task<byte[]> DecryptAsync(byte[] encryptedKey, byte[] passphrase)
        {
            //TODO: implement vault key decryptionm
            await Task.Delay(5);
            return encryptedKey;
        }
    }
}