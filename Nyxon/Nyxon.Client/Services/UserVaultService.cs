using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class UserVaultService : IUserVaultService
    {
        private readonly IVaultDecryptionService _vaultDecryptionService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly EncryptedUserVaultSessionService _vaultSessionService;

        private byte[]? decryptedVaultKey;
        public byte[]? DecryptedVaultKey
        {
            get => decryptedVaultKey;
            private set => decryptedVaultKey = value;
        }
        public bool IsUnlocked => DecryptedVaultKey != null;

        public event Action? StateChanged;

        public UserVaultService(IVaultDecryptionService vaultDecryptionService,
        AuthenticationStateProvider authStateProvider,
        EncryptedUserVaultSessionService vaultSessionService)
        {
            _vaultDecryptionService = vaultDecryptionService;
            _authStateProvider = authStateProvider;
            _vaultSessionService = vaultSessionService;

            // lock on logout
            _authStateProvider.AuthenticationStateChanged += async state =>
            {
                var user = (await state).User;
                if (!user.Identity?.IsAuthenticated ?? true)
                    LockVault();
            };
        }

        public async Task<bool> UnlockVaultAsync(string passphrase)
        {
            if (!_vaultSessionService.HasVault)
                throw new Exception("Vault not loaded");
                
            var decryptedKey = await _vaultDecryptionService.DecryptAsync(_vaultSessionService.EncryptedVaultKey, passphrase);

            // safety net for unauthenticated user
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            if (!state.User.Identity?.IsAuthenticated ?? true)
                return false;

            if (decryptedKey == null)
                return false;

            DecryptedVaultKey = decryptedKey;
            Notify();
            return true;
        }
        public void LockVault()
        {
            DecryptedVaultKey = null;
            Notify();
        }

        private void Notify() => StateChanged?.Invoke();
    }
}