using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IApiService _apiService;
        private readonly IHashService _hashService;
        private readonly ICryptoService _cryptoService;

        public AuthenticationService(AuthenticationStateProvider authStateProvider,
            IApiService apiService,
            IHashService hashService,
            ICryptoService cryptoService)
        {
            _authStateProvider = authStateProvider;
            _apiService = apiService;
            _hashService = hashService;
            _cryptoService = cryptoService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var request = new LoginRequest
            {
                Username = username,
                PasswordHash = _hashService.HashPassword(password)
            };

            try
            {
                // loginresponse -> id, token
                var response = await _apiService.PostAsync<LoginResponse, LoginRequest>("api/auth/login", request);
                return response != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> RegisterAsync(string username, string password, string inviteCode, byte[] passphrase)
        {
            var passwordSalt = _cryptoService.GeneratePasswordSalt();
            var passphraseSalt = _cryptoService.GeneratePassphraseSalt();
            var passphraseKey = _cryptoService.DerivePassphraseKey(passphrase, passphraseSalt);
            var vaultKey = _cryptoService.GenerateVaultKey();
            var identityKey = _cryptoService.GenerateIdentityKey();
            var prekeyBundle = _cryptoService.GeneratePrekeyBundle(identityKey.PrivateKey, vaultKey, 100);

            var encryptedVaultKey = _cryptoService.EncryptWithKey(vaultKey, passphraseKey);
            var encryptedPrivateIdentityKey = _cryptoService.EncryptWithKey(identityKey.PrivateKey, vaultKey);


            var request = new RegisterRequest()
            {
                Username = username,
                PasswordHash = _hashService.HashPassword(password),
                InviteCode = inviteCode,
                PasswordSalt = passwordSalt,
                PublicIdentityKey = identityKey.PublicKey,
                EncryptedVaultKey = encryptedVaultKey,
                EncryptedPrivateIdentityKey = encryptedPrivateIdentityKey,
                PassphraseSalt = passphraseSalt,
                PrekeyBundle = prekeyBundle
            };

            try
            {
                // loginresponse -> id, token
                var response = await _apiService.PostAsync<Guid, RegisterRequest>("api/auth/register", request);
                return response != null;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(passwordSalt);
                CryptographicOperations.ZeroMemory(passphraseSalt);
                CryptographicOperations.ZeroMemory(passphraseKey);
                CryptographicOperations.ZeroMemory(vaultKey);
                CryptographicOperations.ZeroMemory(identityKey.PrivateKey);
                CryptographicOperations.ZeroMemory(encryptedVaultKey);
                CryptographicOperations.ZeroMemory(encryptedPrivateIdentityKey);
            }
        }
        public async Task LogoutAsync()
        {
            await _apiService.PostAsync<object, object>("api/auth/logout", null);
            ((HostAuthenticationStateProvider)_authStateProvider).NotifyStateChanged();
        }
    }
}