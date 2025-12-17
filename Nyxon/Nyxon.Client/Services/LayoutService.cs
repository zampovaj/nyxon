using System;

namespace Nyxon.Client.Services
{
    public class LayoutService
    {
        private readonly IUserVaultService _userVaultService;
        public event Action? OnChange;

        public string HeaderTitle { get; private set; } = "";
        public bool IsVaultUnlocked => _userVaultService.IsUnlocked;

        public LayoutService(IUserVaultService userVaultService)
        {
            _userVaultService = userVaultService;
            _userVaultService.StateChanged += NotifyStateChanged;
        }

        public void SetHeaderTitle(string title)
        {
            HeaderTitle = title;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}