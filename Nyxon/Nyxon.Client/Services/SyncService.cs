using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class SyncService : ISyncService
    {
        private readonly IUserVaultService _userVaultService;
        private readonly IUserListService _userListService;
        private readonly AuthenticationStateProvider _authStateProvider;

        public SyncService(IUserVaultService userVaultService,
            IUserListService userListService,
            AuthenticationStateProvider authStateProvider)
        {
            _userVaultService = userVaultService;
            _userListService = userListService;
            _authStateProvider = authStateProvider;

            _authStateProvider.AuthenticationStateChanged += async task =>
            {
                var state = await task;
                var user = state.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    await SyncAsync();
                }
                else
                {
                    Clear();
                }
            };
        }

        public async Task SyncAsync()
        {
            await _userVaultService.SyncVaultAsync();
            await _userListService.SyncListAsync();
        }
        public void Clear()
        {
            _userVaultService.Clear();
            _userListService.Clear();
        }
    }
}