using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppState _appState;
        private readonly IApiService _apiService;
        private readonly IHashService _hashService;

        public AuthenticationService(AppState appState, IApiService apiService, IHashService hashService)
        {
            _appState = appState;
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

                var userContext = new UserContext
                {
                    UserId = response.UserId,
                    Username = username,
                    Token = response.Token
                };

                _appState.SetUser(userContext);
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
                var response = await _apiService.PostAsync<Guid, RegisterRequest>("api/auth/login", request);
                if (response == null) return false;
            }
            catch (Exception)
            {
                return false;
            }

            // TODO: implement registration
            await Task.Delay(100);
            return false;
        }
        public async Task LogoutAsync()
        {
            // TODO: clear api token??

            _appState.Logout();
            await Task.CompletedTask;
        }
    }
}