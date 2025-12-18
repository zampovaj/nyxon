using Microsoft.AspNetCore.Components.Web;

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
        private readonly IUserVaultService _userVaultService;
        public event Action? StateChanged;

        public bool IsUnlocked => _layoutService.IsVaultUnlocked;
        //in case terminal gets more states in the future, enum is a better option
        public TerminalMode Mode => IsUnlocked ? TerminalMode.Unlocked : TerminalMode.Locked;
        public string InputString { get; set; } = "";
        public string? ErrorMessage { get; private set; } = "";


        public HomeViewModel(LayoutService layoutService, IUserVaultService userVaultService)
        {
            _layoutService = layoutService;
            _userVaultService = userVaultService;

            _layoutService.OnChange += Notify;
        }

        public async Task UnlockVaultAsync()
        {
            try
            {
                var success = await _userVaultService.UnlockVaultAsync(InputString);

                if (!success) ErrorMessage = "Invalid passphrase";

                InputString = "";
                Notify();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                InputString = "";
                Notify();
            }
        }

        private void Notify() => StateChanged?.Invoke();
        private bool IsInputSafe(string input)
        {
            if (input.Length > 100 || string.IsNullOrWhiteSpace(input))
                return false;
            
            return true;
        }
        public void Dispose()
        {
            _layoutService.OnChange -= Notify;
        }

        public async Task HandleTerminalKey(KeyboardEventArgs e)
        {
            if (e.Key != "Enter")
                return;

            if (!IsInputSafe(InputString))
            {
                ErrorMessage = "Invalid input detected.";
                Notify();
                return;
            }

            if (!IsUnlocked)
            {
                await UnlockVaultAsync();
            }
            else
            {
                // TODO: commands
            }
        }

    }
}