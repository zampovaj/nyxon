using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;

namespace Nyxon.Client.Services
{
    public class EncryptedUserVaultSessionService
    {
        public byte[]? PassphraseSalt { get; private set; } = null;
        public byte[]? EncryptedVaultKey { get; private set; } = null;
        public byte[]? EncryptedPrivateIdentityKey { get; private set; } = null;

        public bool HasVault =>
            EncryptedVaultKey != null &&
            PassphraseSalt != null &&
            EncryptedPrivateIdentityKey != null;

        public void CheckVault()
        {
            Console.WriteLine("PassphraseSalt: " + Convert.ToBase64String(PassphraseSalt) ?? "null");
            Console.WriteLine("EncryptedVaultKey: " + Convert.ToBase64String(EncryptedVaultKey) ?? "null");
            Console.WriteLine("EncryptedPrivateIdentityKey: " + Convert.ToBase64String(EncryptedPrivateIdentityKey) ?? "null");
        }

        public void LoadVault(UserVaultResponse vault)
        {
            PassphraseSalt = vault.PassphraseSalt;
            EncryptedVaultKey = vault.EncryptedVaultKey;
            EncryptedPrivateIdentityKey = vault.EncryptedPrivateIdentityKey;
        }
        public void Clear()
        {
            PassphraseSalt = null;
            EncryptedVaultKey = null;
            EncryptedPrivateIdentityKey = null;
        }
    }
}