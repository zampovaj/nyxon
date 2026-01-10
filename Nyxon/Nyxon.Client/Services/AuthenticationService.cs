using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NSec.Cryptography;
using Nyxon.Client.Interfaces;
using Nyxon.Core.Services;

namespace Nyxon.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IApiService _apiService;
        private readonly IHashService _hashService;
        private readonly ICryptoService _cryptoService;
        private readonly CsrfTokenStore _csrfTokenStore;

        public AuthenticationService(AuthenticationStateProvider authStateProvider,
            IApiService apiService,
            IHashService hashService,
            ICryptoService cryptoService,
            CsrfTokenStore csrfTokenStore)
        {
            _authStateProvider = authStateProvider;
            _apiService = apiService;
            _hashService = hashService;
            _cryptoService = cryptoService;
            _csrfTokenStore = csrfTokenStore;
        }

        public async Task<bool> LoginAsync(string username, byte[] password)
        {
            Console.WriteLine("Login started");
            //_csrfTokenStore.Check();
            _csrfTokenStore.Clear();

            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var isAuthenticated = state.User.Identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
                await LogoutAsync();

            var request = new LoginRequest
            {
                Username = username,
                PasswordHash = _hashService.HashPassword(password)
            };

            try
            {
                // loginresponse -> id, token
                var response = await _apiService.PostAsync<LoginResponse, LoginRequest>("api/auth/login", request);
                Console.WriteLine("Login finished");
                return response != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> RegisterAsync(string username, byte[] password, string inviteCode, byte[] passphrase)
        {
            //_csrfTokenStore.Check();
            _csrfTokenStore.Clear();

            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var isAuthenticated = state.User.Identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
                await LogoutAsync();

            var userId = Guid.NewGuid();

            byte[]? passphraseKey = null;
            byte[]? vaultKey = null;
            AsymmetricKey? identityKey = null;
            AsymmetricKey? agreementKey = null;

            try
            {
                var passwordSalt = _cryptoService.GeneratePasswordSalt();
                var passphraseSalt = _cryptoService.GeneratePassphraseSalt();

                passphraseKey = await _cryptoService.DerivePassphraseKeyAsync(passphrase, passphraseSalt);
                vaultKey = _cryptoService.GenerateVaultKey();
                identityKey = _cryptoService.GenerateIdentityKey();
                agreementKey = _cryptoService.GenerateAgreementKey();
                var prekeyBundle = _cryptoService.GeneratePrekeyBundle(identityKey.PrivateKey, vaultKey, 100, userId);

                var encryptedVaultKey = _cryptoService.EncryptWithKey(
                    vaultKey,
                    passphraseKey,
                    AadFactory.ForUserVaultKey(userId)
                );
                var encryptedPrivateIdentityKey = _cryptoService.EncryptWithKey(
                    identityKey.PrivateKey,
                    vaultKey,
                    AadFactory.ForIdentityKey(userId)
                );
                var encryptedPrivateAgreementKey = _cryptoService.EncryptWithKey(
                    agreementKey.PrivateKey,
                    vaultKey,
                    AadFactory.ForAgreementKey(userId)
                );


                var request = new RegisterRequest()
                {
                    Id = userId,
                    Username = username,
                    PasswordHash = _hashService.HashPassword(password),
                    InviteCode = inviteCode,
                    PasswordSalt = passwordSalt,
                    PublicIdentityKey = identityKey.PublicKey,
                    PublicAgreementKey = agreementKey.PublicKey,
                    EncryptedVaultKey = encryptedVaultKey,
                    EncryptedPrivateIdentityKey = encryptedPrivateIdentityKey,
                    EncryptedPrivateAgreementKey = encryptedPrivateAgreementKey,
                    PassphraseSalt = passphraseSalt,
                    PrekeyBundle = prekeyBundle
                };

                // loginresponse -> id, token
                var response = await _apiService.PostAsync<LoginResponse, RegisterRequest>("api/auth/register", request);
                return response != null;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (passphraseKey != null) CryptographicOperations.ZeroMemory(passphraseKey);
                if (vaultKey != null) CryptographicOperations.ZeroMemory(vaultKey);
                if (identityKey != null && identityKey.PrivateKey != null) CryptographicOperations.ZeroMemory(identityKey.PrivateKey);
                if (agreementKey != null && agreementKey.PrivateKey != null) CryptographicOperations.ZeroMemory(agreementKey.PrivateKey);
            }
        }
        public async Task LogoutAsync()
        {
            await _apiService.PostAsync<object, object>("api/auth/logout", null);
            //_userVaultService.Clear();
            ((HostAuthenticationStateProvider)_authStateProvider).NotifyStateChanged();
        }
    }
}