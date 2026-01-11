using System;

namespace Nyxon.Client.Services
{
    public class LayoutService : IDisposable
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
            if (!IsVaultUnlocked) return;

            HeaderTitle = title;
            NotifyStateChanged();
        }

        public void Clear()
        {
            HeaderTitle = "";
            NotifyStateChanged();
        }

        public void Dispose()
        {
            Clear();
            _userVaultService.StateChanged -= NotifyStateChanged;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}