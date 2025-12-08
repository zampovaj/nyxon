using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Services;

namespace Nyxon.Client.Services
{
    public class LayoutService
    {
        public bool IsLoggedIn { get; private set; } = false;
        public bool IsVaultUnlocked { get; private set; } = false;

        public string? HeaderTitle { get; private set; } = "";
        public string Username { get; private set; } = "";
        public event Action? OnChange;

        public void SetLoginState(bool isLoggedIn)
        {
            IsLoggedIn = isLoggedIn;
            if (!isLoggedIn) HeaderTitle = null;
            NotifyListeners();
        }

        // TODO: actually check
        public void UnlockVault(string passphrase)
        {
            if (!string.IsNullOrWhiteSpace(passphrase))
            {
                IsVaultUnlocked = true;
                NotifyListeners();
            }
        }
        public void SetHeaderTitle(string? headerTitle)
        {
            HeaderTitle = headerTitle;
            NotifyListeners();
        }
        private void NotifyListeners() => OnChange?.Invoke();
    }
}