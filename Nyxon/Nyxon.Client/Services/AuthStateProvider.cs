using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Nyxon.Client.State;

namespace Nyxon.Client.Services
{
    public class AuthStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly AppState _appState;

        public AuthStateProvider(AppState appState)
        {
            _appState = appState;
            _appState.OnChange += OnAppStateChanged;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = _appState.CurrentUser;

            if (user != null && user.IsValid)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                };

                var identity = new ClaimsIdentity(claims, "NyxonAuth");
                var userPrincipal = new ClaimsPrincipal(identity);

                return Task.FromResult(new AuthenticationState(userPrincipal));
            }
            else
            {
                var anonymousPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
                return Task.FromResult(new AuthenticationState(anonymousPrincipal));
            }
        }

        private void OnAppStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Dispose()
        {
            _appState.OnChange -= OnAppStateChanged;
        }
    }
}