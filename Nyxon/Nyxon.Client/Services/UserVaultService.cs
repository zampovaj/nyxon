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
    public class UserVaultService : IUserVaultService, IDisposable
    {
        private readonly UserContext _userContext;
        private readonly EncryptedUserVaultSessionService _vaultSessionService;
        private readonly IVaultRepository _vaultRepository;
        private readonly ICryptoService _cryptoService;

        private byte[]? DecryptedVaultKey { get; set; } = null;
        private byte[]? DecryptedPrivateIdentityKey { get; set; } = null;
        private byte[]? DecryptedPrivateAgreementKey { get; set; } = null;
        public bool IsUnlocked => DecryptedVaultKey != null;

        public event Action? StateChanged;

        public UserVaultService(UserContext userContext,
            EncryptedUserVaultSessionService vaultSessionService,
            IVaultRepository vaultRepository,
            ICryptoService cryptoService)
        {
            _userContext = userContext;
            _vaultSessionService = vaultSessionService;
            _vaultRepository = vaultRepository;
            _cryptoService = cryptoService;
        }

        public async Task<bool> UnlockVaultAsync(byte[] passphrase)
        {
            // safety net for unauthenticated user
            if (!_userContext.IsAuthenticated)
                return false;

            var userId = (Guid)_userContext.UserId;

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

                if (DecryptedVaultKey == null)
                    return false;
                
                // do spk check
                await CheckAndGenerateSpkAsync();

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
            if (!_vaultSessionService.HasVault && _userContext.IsAuthenticated)
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
            if (!_userContext.IsAuthenticated || !IsUnlocked)
                throw new UnauthorizedAccessException("User must be authenticated to derive shared secret.");

            try
            {
                DecryptedPrivateAgreementKey = _cryptoService.DecryptWithKey(
                    _vaultSessionService.EncryptedPrivateAgreementKey,
                    DecryptedVaultKey,
                    AadFactory.ForAgreementKey((Guid)_userContext.UserId));

                if (DecryptedPrivateAgreementKey == null)
                    throw new InvalidOperationException("Failed to decrypt private agreement key.");

                return _cryptoService.DeriveSharedSecret(DecryptedPrivateAgreementKey, publicKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in " + nameof(CalculateIdentityDhAsync) + ": " + ex.Message);
                throw;
            }
            finally
            {
                if (DecryptedPrivateAgreementKey != null) CryptographicOperations.ZeroMemory(DecryptedPrivateAgreementKey);
            }
        }

        public async Task<byte[]> SignAsync(byte[] data)
        {
            if (!_userContext.IsAuthenticated || !IsUnlocked)
                throw new UnauthorizedAccessException("User must be authenticated to sign data.");

            try
            {
                DecryptedPrivateIdentityKey = _cryptoService.DecryptWithKey(
                    _vaultSessionService.EncryptedPrivateIdentityKey,
                    DecryptedVaultKey,
                    AadFactory.ForIdentityKey((Guid)_userContext.UserId));

                if (DecryptedPrivateIdentityKey == null)
                    throw new InvalidOperationException("Failed to decrypt private identity key.");

                return _cryptoService.SignData(data, DecryptedPrivateIdentityKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in "+ nameof(SignAsync) + ": " + ex.Message);
                throw;
            }
            finally
            {
                if (DecryptedPrivateIdentityKey != null) CryptographicOperations.ZeroMemory(DecryptedPrivateIdentityKey);
            }
        }

        public void Clear()
        {
            LockVault();
            _vaultSessionService.Clear();
            Notify();
        }

        public void Dispose()
        {
            Clear();
        }

        public void CheckEncryptedVault()
        {
            _vaultSessionService.CheckVault();
        }

        private void Notify() => StateChanged?.Invoke();

        private async Task CheckAndGenerateSpkAsync()
        {
            if (!_userContext.IsAuthenticated || !IsUnlocked)
                throw new UnauthorizedAccessException("User must be authenticated to sign data.");

            byte[]? decryptedIdentityKey = null;
            SignedPrekey? spk = null;

            try
            {
                var response = await _vaultRepository.CheckSignedPrekeyAsync();
                if (response)
                {
                    decryptedIdentityKey = _cryptoService.DecryptWithKey(_vaultSessionService.EncryptedPrivateIdentityKey, DecryptedVaultKey, AadFactory.ForIdentityKey((Guid)_userContext.UserId));
                    if (decryptedIdentityKey == null) throw new Exception("Failed to decrypt private identity key");
                    spk = _cryptoService.GenerateSpk(decryptedIdentityKey, DecryptedVaultKey, (Guid)_userContext.UserId);
                    await _vaultRepository.SaveNewSpkAsync(spk);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating new signed prekey: {ex.Message}");
            }
            finally
            {
                if (decryptedIdentityKey != null) CryptographicOperations.ZeroMemory(decryptedIdentityKey);
                if (spk != null && spk.PrivateKey != null) CryptographicOperations.ZeroMemory(spk.PrivateKey);
            }
        }
    }
}