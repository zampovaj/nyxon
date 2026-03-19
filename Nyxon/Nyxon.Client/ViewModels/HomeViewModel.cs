using System.Text;
using Microsoft.AspNetCore.Components.Web;
using System.Security.Cryptography;

namespace Nyxon.Client.ViewModels
{
    public enum TerminalMode
    {
        Locked,
        Unlocked,
        Command,
        Mention
    }
    public class HomeViewModel : IDisposable
    {
        private readonly LayoutService _layoutService;
        private readonly IUserVaultService _userVaultService;
        private readonly IUserListService _userListService;
        private readonly IInboxService _inboxService;
        private readonly NavigationManager _nav;
        private readonly IConversationRepository _conversationRepository;
        public event Action? StateChanged;

        public bool IsUnlocked => _layoutService.IsVaultUnlocked;
        //in case terminal gets more states in the future, enum is a better option
        public TerminalMode Mode
        {
            get
            {
                if (!IsUnlocked) return TerminalMode.Locked;
                if (!string.IsNullOrWhiteSpace(InputString))
                {
                    if (InputString.StartsWith(":")) return TerminalMode.Command;
                    if (InputString.StartsWith("@")) return TerminalMode.Mention;
                }
                return TerminalMode.Unlocked;
            }
        }
        private byte[] PassphraseBytes = Array.Empty<byte>();
        private string _inputString = "";
        public string InputString
        {
            get => _inputString;
            set
            {
                if (_inputString == value) return;
                _inputString = value;

                switch (Mode)
                {
                    case TerminalMode.Unlocked:
                        SearchResults = _userListService.SearchUsers(_inputString);
                        MentionResults.Clear();
                        break;

                    case TerminalMode.Mention:
                        var usernamePart = _inputString[1..]; // skip @
                        MentionResults = _inboxService.SearchConversations(usernamePart);
                        SearchResults.Clear();
                        break;

                    case TerminalMode.Command:
                        SearchResults.Clear();
                        MentionResults.Clear();
                        break;
                }

                Notify();
            }

        }
        private readonly Dictionary<string, Func<string[], Task>> _commands;

        public string? ErrorMessage { get; private set; } = "";
        public List<string>? InviteCodes { get; set; } = null;
        public List<UserModel> SearchResults = new();
        public List<Conversation> MentionResults = new();


        public HomeViewModel(LayoutService layoutService,
            IUserVaultService userVaultService,
            IUserListService userListService,
            NavigationManager nav,
            IInboxService inboxService,
            IConversationRepository conversationRepository)
        {
            _layoutService = layoutService;
            _userVaultService = userVaultService;
            _userListService = userListService;
            _nav = nav;
            _inboxService = inboxService;
            _conversationRepository = conversationRepository;

            _layoutService.OnChange += Notify;

            _commands = new()
            {
                [":invite"] = async args =>
                {
                    int count = 1;

                    if (args.Length > 0 && int.TryParse(args[^1], out var parsed))
                        count = Math.Clamp(parsed, 1, 20);

                    await GenerateInvites(count);
                }
            };
        }

        public async Task UnlockVaultAsync()
        {
            try
            {
                PassphraseBytes = Encoding.UTF8.GetBytes(InputString);
                var success = await _userVaultService.UnlockVaultAsync(PassphraseBytes);

                // _userVaultService.CheckDecryptedKeys();

                if (!success) ErrorMessage = "Invalid passphrase";
                else
                {
                    ErrorMessage = "";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                InputString = string.Empty;
                CryptographicOperations.ZeroMemory(PassphraseBytes);
                GC.Collect(2, GCCollectionMode.Forced, blocking: false);
                Notify();
            }
        }
        private bool IsInputSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            int maxLength = IsUnlocked ? 160 : 256;
            int minLength = IsUnlocked ? 1 : 16;

            return input.Length >= minLength && input.Length <= maxLength;
        }

        public void Dispose()
        {
            _layoutService.OnChange -= Notify;
            CryptographicOperations.ZeroMemory(PassphraseBytes);
            InputString = "";
            ErrorMessage = "";
        }

        public async Task HandleTerminalKey(KeyboardEventArgs e)
        {
            if (e.Key != "Enter")
                return;

            ErrorMessage = "";

            if (!IsInputSafe(InputString))
            {
                ErrorMessage = "Invalid input detected.";
                Notify();
                return;
            }

            if (Mode == TerminalMode.Locked)
            {
                await UnlockVaultAsync();
                //_userVaultService.CheckEncryptedVault();
                return;
            }
            if (Mode == TerminalMode.Unlocked)
            {
                if (SearchResults.Any(s => s.Username.ToLower() == InputString.ToLower()))
                {
                    _nav.NavigateTo($"chat/{InputString}");
                }
            }
            if (Mode == TerminalMode.Command)
            {
                var parts = InputString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && _commands.TryGetValue(parts[0].ToLower(), out var handler))
                    await handler(parts.Skip(1).ToArray());
                else
                    ErrorMessage = $"Unknown command: {parts[0]}";

                InputString = "";
                Notify();
                return;
            }
            if (Mode == TerminalMode.Mention)
            {
                // TODO: send message
            }
        }

        private async Task GenerateInvites(int count = 1)
        {
            if (count < 1)
            {
                ErrorMessage = "Cannot generate negative ammount on invite codes";
                Notify();
                return;
            }
            InviteCodes = await _conversationRepository.GenerateInviteCodesAsync(count);
            Notify();
        }

        public void ClearError()
        {
            ErrorMessage = "";
        }
        private void Notify() => StateChanged?.Invoke();
    }
}