using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        private byte[]? DecryptedVaultKey { get; set; } = null;
        private byte[]? DecryptedPrivateIdentityKey { get; set; } = null;
        public bool IsUnlocked => DecryptedVaultKey != null && DecryptedPrivateIdentityKey != null;

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
        }

        public async Task<bool> UnlockVaultAsync(byte[] passphrase)
        {
            // safety net for unauthenticated user
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            if (!state.User.Identity?.IsAuthenticated ?? true)
                return false;

            if (!_vaultSessionService.HasVault)
            {
                var sync = await SyncVaultAsync();
                if (!sync) return false;
            }

            try
            {
                var passphraseKey = await _cryptoService.DerivePassphraseKeyAsync(passphrase, _vaultSessionService.PassphraseSalt);

                if (passphraseKey == null) return false;

                DecryptedVaultKey = _cryptoService.DecryptWithKey(_vaultSessionService.EncryptedVaultKey, passphraseKey);
                DecryptedPrivateIdentityKey = _cryptoService.DecryptWithKey(_vaultSessionService.EncryptedPrivateIdentityKey, DecryptedVaultKey);

                if (DecryptedVaultKey == null || DecryptedPrivateIdentityKey == null)
                    return false;

                Notify();
                return IsUnlocked;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(passphrase);
            }
        }
        public void LockVault()
        {
            if (DecryptedPrivateIdentityKey != null)
                CryptographicOperations.ZeroMemory(DecryptedPrivateIdentityKey);
            if (DecryptedVaultKey != null)
                CryptographicOperations.ZeroMemory(DecryptedVaultKey);

            DecryptedVaultKey = null;
            DecryptedPrivateIdentityKey = null;
            Notify();
        }

        public async Task<bool> SyncVaultAsync()
        {
            if (!_vaultSessionService.HasVault)
            {
                var userVault = await _vaultRepository.FetchUserVaultAsync();
                if (userVault == null)
                {
                    Console.WriteLine("vault: null");
                    return false;
                }

                _vaultSessionService.LoadVault(userVault);
            }
            return true;
        }

        public async Task<byte[]> DecryptAsync(byte[] data)
        {
            if (!IsUnlocked)
                throw new UnauthorizedAccessException("Vault needs to be decrypted to decrypt ciphertext using vault key.");

            return _cryptoService.DecryptWithKey(data, DecryptedVaultKey);
        }
        public async Task<byte[]> EncryptAsync(byte[] data)
        {
            if (!IsUnlocked)
                throw new UnauthorizedAccessException("Vault needs to be decrypted to encrypt plaintext using vault key.");

            return _cryptoService.EncryptWithKey(data, DecryptedVaultKey);
        }

        public void Clear()
        {
            LockVault();
            _vaultSessionService.Clear();
            Notify();
        }

        public void CheckEncryptedVault()
        {
            _vaultSessionService.CheckVault();
        }

        private void Notify() => StateChanged?.Invoke();
    }
}