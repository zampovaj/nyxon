using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IApiService _apiService;
        private readonly IHashService _hashService;

        public AuthenticationService(AuthenticationStateProvider authStateProvider, IApiService apiService, IHashService hashService)
        {
            _authStateProvider = authStateProvider;
            _apiService = apiService;
            _hashService = hashService;
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
                if (response == null) return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> RegisterAsync(string username, string password, string inviteCode)
        {
            var request = new RegisterRequest
            {
                Username = username,
                PasswordHash = _hashService.HashPassword(password),
                InviteCode = inviteCode
            };

            try
            {
                // loginresponse -> id, token
                var response = await _apiService.PostAsync<Guid, RegisterRequest>("api/auth/register", request);
                if (response != null) return true;
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
        public async Task LogoutAsync()
        {
            await _apiService.PostAsync<object, object>("api/auth/logout", null);
            ((HostAuthenticationStateProvider)_authStateProvider).NotifyStateChanged();
        }
    }
}