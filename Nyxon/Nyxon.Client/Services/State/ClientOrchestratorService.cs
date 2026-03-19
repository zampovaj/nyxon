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
        private readonly INotificationService _notificationService;
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
            INotificationService notificationService,
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
            _notificationService = notificationService;
            _nav = nav;
        }

        public async Task Initialize()
        {
            if (_isInitialized) return;

            _authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
            _isInitialized = true;

            await _notificationService.InitializeAsync();

            _ = CheckAndSyncAsync();
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            await CheckAndSyncAsync();
            _hubService.CheckHubState();
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

                        await _hubService.ConnectAsync();
                        await _notificationService.InitializeAsync();

                        await _userVaultService.SyncVaultAsync();
                        await _inboxService.SyncInboxAsync();
                        await _userListService.SyncListAsync();
                    }
                }
                else
                {
                    await ClearServicesAsync();
                    await _hubService.DisconnectAsync();
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
            _ = DisposeServicesAsync();
        }

        private async Task ClearServicesAsync()
        {
            _userContext.Clear();
            _userVaultService.Clear();
            _userListService.Clear();
            _handshakeService.Clear();
            _inboxService.Clear();
            _csrfTokenStore.Clear();
            _activeConversationService.Clear();
            await _hubService.DisconnectAsync();
            _layoutService.Clear();
            _notificationService.Clear();
        }

        private async Task DisposeServicesAsync()
        {
            _userContext.Dispose();
            _userVaultService.Dispose();
            _userListService.Dispose();
            _handshakeService.Dispose();
            _inboxService.Dispose();
            _activeConversationService.Dispose();
            _csrfTokenStore.Clear();
            await _hubService.DisconnectAsync();
            _layoutService.Dispose();
            _notificationService.Dispose();
        }
    }
}