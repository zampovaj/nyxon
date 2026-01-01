using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Nyxon.Core.Services;

namespace Nyxon.Client.Services
{
    public class UserVaultService : IUserVaultService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly EncryptedUserVaultSessionService _vaultSessionService;
        private readonly IVaultRepository _vaultRepository;
        private readonly ICryptoService _cryptoService;

        private byte[]? DecryptedVaultKey { get; set; } = null;
        private byte[]? DecryptedPrivateIdentityKey { get; set; } = null;
        public bool IsUnlocked => DecryptedVaultKey != null && DecryptedPrivateIdentityKey != null;

        public event Action? StateChanged;

        public UserVaultService(AuthenticationStateProvider authStateProvider,
            EncryptedUserVaultSessionService vaultSessionService,
            IVaultRepository vaultRepository,
            ICryptoService cryptoService)
        {
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

            var userIdString = state.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null || !Guid.TryParse(userIdString, out var userId))
                return false;

            if (!_vaultSessionService.HasVault)
            {
                var sync = await SyncVaultAsync();
                if (!sync) return false;
            }

            byte[]? passphraseKey = null;

            try
            {
                passphraseKey = await _cryptoService.DerivePassphraseKeyAsync(passphrase, _vaultSessionService.PassphraseSalt);

                if (passphraseKey == null) return false;
                DecryptedVaultKey = _cryptoService.DecryptWithKey(
                    _vaultSessionService.EncryptedVaultKey,
                    passphraseKey,
                    AadFactory.ForUserVaultKey(userId));
                DecryptedPrivateIdentityKey = _cryptoService.DecryptWithKey(
                    _vaultSessionService.EncryptedPrivateIdentityKey,
                    DecryptedVaultKey,
                    AadFactory.ForIdentityKey(userId));

                if (DecryptedVaultKey == null || DecryptedPrivateIdentityKey == null)
                    return false;

                Notify();
                return IsUnlocked;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(passphrase);
                CryptographicOperations.ZeroMemory(passphraseKey);
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

        public async Task<byte[]> DecryptAsync(byte[] data, byte[]? aad = null)
        {
            if (!IsUnlocked)
                throw new UnauthorizedAccessException("Vault needs to be decrypted to decrypt ciphertext using vault key.");

            return _cryptoService.DecryptWithKey(data, DecryptedVaultKey, aad);
        }
        public async Task<byte[]> EncryptAsync(byte[] data, byte[]? aad = null)
        {
            if (!IsUnlocked)
                throw new UnauthorizedAccessException("Vault needs to be decrypted to encrypt plaintext using vault key.");

            return _cryptoService.EncryptWithKey(data, DecryptedVaultKey, aad);
        }

        public async Task<byte[]> CalculateIdentityDhAsync(byte[] publicKey)
        {
            if (!IsUnlocked)
                throw new UnauthorizedAccessException("Vault needs to be decrypted to derive shared secret.");

            return _cryptoService.DeriveSharedSecret(DecryptedPrivateIdentityKey, publicKey);
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

        //TODO: remove this shit!!!
        /*public void CheckDecryptedKeys()
        {
            Console.WriteLine("DecryptedVaultKey: " + (DecryptedVaultKey == null ? "null" : Convert.ToBase64String(DecryptedVaultKey)));
            Console.WriteLine("DecryptedPrivateIdentityKey: " + (DecryptedPrivateIdentityKey == null ? "null" : Convert.ToBase64String(DecryptedPrivateIdentityKey)));
        }*/

        private void Notify() => StateChanged?.Invoke();
    }
}