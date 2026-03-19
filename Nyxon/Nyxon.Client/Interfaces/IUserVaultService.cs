using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IUserVaultService : IDisposable
    {
        bool IsUnlocked { get; }
        event Action? StateChanged;
        Task<bool> SyncVaultAsync();
        Task<bool> UnlockVaultAsync(byte[] passphrase);
        void LockVault();
        Task<byte[]> DecryptAsync(byte[] data, byte[]? aad = null);
        Task<byte[]> EncryptAsync(byte[] data, byte[]? aad = null);
        Task<byte[]> CalculateIdentityDhAsync(byte[] publicKey);
        Task<byte[]> SignAsync(byte[] data);
        void Clear();
        void CheckEncryptedVault();
    }
}