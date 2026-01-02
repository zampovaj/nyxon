using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Nyxon.Client.Services
{
    public class ClientOrchestratorService : IDisposable
    {
        private readonly UserContext _userContext;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IUserVaultService _userVaultService;
        private readonly IUserListService _userListService;
        private readonly IHandshakeService _handshakeService;
        private readonly IInboxService _inboxService;

        private bool _isInitialized;

        public ClientOrchestratorService(UserContext userContext,
            AuthenticationStateProvider authStateProvider,
            IUserVaultService userVaultService,
            IUserListService userListService,
            IHandshakeService handshakeService,
            IInboxService inboxService)
        {
            _userContext = userContext;
            _authStateProvider = authStateProvider;
            _userVaultService = userVaultService;
            _userListService = userListService;
            _handshakeService = handshakeService;
            _inboxService = inboxService;
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

                        await _userVaultService.SyncVaultAsync();
                        await _inboxService.SyncInboxAsync();
                        await _userListService.SyncListAsync();
                    }
                }
                else
                {
                    _userContext.Clear();
                    _userVaultService.Clear();
                    _userListService.Clear();
                    _handshakeService.Clear();
                    _inboxService.Clear();
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
        }
    }
}