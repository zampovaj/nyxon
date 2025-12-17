using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class EncryptedUserVaultSessionService
    {
        public byte[]? EncryptedVaultKey { get; private set; }
        public bool HasVault => EncryptedVaultKey != null;

        public void SetEncryptedVaultKey(byte[] encryptedVaultKey)
        {
            EncryptedVaultKey = encryptedVaultKey;
        }
        public void Clear()
        {
            EncryptedVaultKey = null;
        }
    }
}