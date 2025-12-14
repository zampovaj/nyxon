using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Nyxon.Client.Services
{
    public class HostAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public HostAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSession = await _httpClient.GetFromJsonAsync<UserSessionDto>("api/auth/me");

                if (userSession != null && userSession.IsAuthenticated)
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, userSession.Username),
                        new Claim(ClaimTypes.NameIdentifier, userSession.UserId)
                    };

                    //logged in
                    var identity = new ClaimsIdentity(claims, "ServerCookie");
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                }
            }
            //anonymous
            catch { }
            return new AuthenticationState(new ClaimsPrincipal());
        }
        public void NotifyStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}