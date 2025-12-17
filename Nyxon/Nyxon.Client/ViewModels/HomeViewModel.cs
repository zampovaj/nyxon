using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.ViewModels
{
    public enum TerminalMode
    {
        Locked,
        Unlocked
    }
    public class HomeViewModel : IDisposable
    {
        private readonly LayoutService _layoutService;
        private readonly UserVaultService _userVaultService;
        public event Action? StateChanged;

        public bool IsUnlocked => _layoutService.IsVaultUnlocked;
        //in case terminal gets more states in the future, enum is a better option
        public TerminalMode Mode => IsUnlocked ? TerminalMode.Unlocked : TerminalMode.Locked;
        public string InputMessage { get; set; } = "";
        public string? ErrorMessage { get; private set; } = "";
        public string Passphrase { get; set; } = "";


        public HomeViewModel(LayoutService layoutService, UserVaultService userVaultService)
        {
            _layoutService = layoutService;
            _userVaultService = userVaultService;

            _layoutService.OnChange += Notify;
        }

        public async Task UnlockVaultAsync()
        {
            try
            {
                var success = await _userVaultService.UnlockVaultAsync(Passphrase);

                if (!success) ErrorMessage = "Invalid passphrase";

                Passphrase = "";
                Notify();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Passphrase = "";
                Notify();
            }
        }

        private void Notify() => StateChanged?.Invoke();
        public void Dispose()
        {
            _layoutService.OnChange -= Notify;
        }
    }
}