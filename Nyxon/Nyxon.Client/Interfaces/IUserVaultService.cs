using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IUserVaultService
    {
        bool IsUnlocked { get; }
        event Action? StateChanged;
        Task<bool> SyncVaultAsync();
        Task<bool> UnlockVaultAsync(byte[] passphrase);
        void LockVault();
    }
}