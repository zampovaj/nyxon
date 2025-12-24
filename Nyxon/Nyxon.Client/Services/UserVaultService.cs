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
        private readonly IVaultRepository _vaultRepository;
        private readonly ICryptoService _cryptoService;

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
        EncryptedUserVaultSessionService vaultSessionService,
        IVaultRepository vaultRepository,
        ICryptoService cryptoService)
        {
            _vaultDecryptionService = vaultDecryptionService;
            _authStateProvider = authStateProvider;
            _vaultSessionService = vaultSessionService;
            _vaultRepository = vaultRepository;
            _cryptoService = cryptoService;

            // lock on logout
            _authStateProvider.AuthenticationStateChanged += async state =>
            {
                var user = (await state).User;
                if (!user.Identity?.IsAuthenticated ?? true)
                    LockVault();
            };
        }

        public async Task<bool> UnlockVaultAsync(byte[] passphrase)
        {
            if (!_vaultSessionService.HasVault)
            {
                var sync = await SyncVaultAsync();
                if (!sync) return false;
            }

            var decryptedVaultKey = _cryptoService.DecryptKey(_vaultSessionService.EncryptedVaultKey, passphrase);

            // safety net for unauthenticated user
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            if (!state.User.Identity?.IsAuthenticated ?? true)
                return false;

            if (decryptedVaultKey == null)
                return false;

            DecryptedVaultKey = decryptedVaultKey;
            Notify();
            return true;
        }
        public void LockVault()
        {
            DecryptedVaultKey = null;
            Notify();
        }

        public async Task<bool> SyncVaultAsync()
        {
            var userVault = await _vaultRepository.FetchUserVaultAsync();
            if (userVault == null)
            {
                Console.WriteLine("vault: null");
                return false;
            }

            _vaultSessionService.LoadVault(userVault);
            return true;
        }

        private void Notify() => StateChanged?.Invoke();
    }
}