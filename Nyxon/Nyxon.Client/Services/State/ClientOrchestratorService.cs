using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Principal;

namespace Nyxon.Client.Services.State
{
    public class ClientOrchestratorService : IDisposable
    {
        private readonly UserContext _userContext;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IUserVaultService _userVaultService;
        private readonly IUserListService _userListService;
        private readonly IHandshakeService _handshakeService;
        private readonly IInboxService _inboxService;
        private readonly CsrfTokenStore _csrfTokenStore;
        private readonly IActiveConversationService _activeConversationService;
        private readonly IHubService _hubService;
        private readonly LayoutService _layoutService;
        private readonly IAuthenticationService _authenticationService;
        private readonly NavigationManager _nav;

        private bool _isInitialized;

        public ClientOrchestratorService(UserContext userContext,
            AuthenticationStateProvider authStateProvider,
            IUserVaultService userVaultService,
            IUserListService userListService,
            IHandshakeService handshakeService,
            IInboxService inboxService,
            CsrfTokenStore csrfTokenStore,
            IActiveConversationService activeConversationService,
            IHubService hubService,
            LayoutService layoutService,
            IAuthenticationService authorizationService,
            NavigationManager nav)
        {
            _userContext = userContext;
            _authStateProvider = authStateProvider;
            _userVaultService = userVaultService;
            _userListService = userListService;
            _handshakeService = handshakeService;
            _inboxService = inboxService;
            _csrfTokenStore = csrfTokenStore;
            _activeConversationService = activeConversationService;
            _hubService = hubService;
            _layoutService = layoutService;
            _authenticationService = authorizationService;
            _nav = nav;
        }

        public async Task Initialize()
        {
            if (_isInitialized) return;

            _authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
            _isInitialized = true;

            _ = CheckAndSyncAsync();
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            await CheckAndSyncAsync();
        }

        private async Task CheckAndSyncAsync()
        {
            try
            {
                var state = await _authStateProvider.GetAuthenticationStateAsync();
                var user = state.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var username = user.FindFirst(ClaimTypes.Name)?.Value;

                    if (Guid.TryParse(userIdString, out var userId))
                    {
                        _userContext.SetUser(userId, username);

                        _csrfTokenStore.Clear();

                        await _userVaultService.SyncVaultAsync();
                        await _inboxService.SyncInboxAsync();
                        await _userListService.SyncListAsync();
                    }
                }
                else
                {
                    ClearServices();

                    Console.WriteLine("Everything clear");
                    Console.WriteLine("Token: " + (_csrfTokenStore.Token ?? "null"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Orchestrator Error: {ex.Message}");
            }
        }
        public void Dispose()
        {
            _authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            DisposeServices();
        }

        private void ClearServices()
        {
            _userContext.Clear();
            _userVaultService.Clear();
            _userListService.Clear();
            _handshakeService.Clear();
            _inboxService.Clear();
            _csrfTokenStore.Clear();
            _activeConversationService.Clear();
            _hubService.DisconnectAsync().GetAwaiter().GetResult();
            _layoutService.Clear();
        }

        private void DisposeServices()
        {
            _userContext.Dispose();
            _userVaultService.Dispose();
            _userListService.Dispose();
            _handshakeService.Dispose();
            _inboxService.Dispose();
            _activeConversationService.Dispose();
            _csrfTokenStore.Clear();
            _hubService.DisconnectAsync().GetAwaiter().GetResult();
            _layoutService.Dispose();
        }
    }
}