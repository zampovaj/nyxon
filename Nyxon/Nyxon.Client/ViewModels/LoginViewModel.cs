using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Nyxon.Client.Interfaces;
using System.Text;

namespace Nyxon.Client.ViewModels
{
    public class LoginViewModel : IDisposable
    {
        private readonly IAuthenticationService _authService;
        private readonly NavigationManager _nav;
        private readonly AuthenticationStateProvider _authStateProvider;

        [Required]
        private string username = "";
        public string Username
        {
            get => username;
            set => username = value.Trim();
        }
        [Required]
        private string password = "";
        public string Password
        {
            get => password;
            set => password = value.Trim();
        }
        private byte[]? passwordBytes = null;
        private string confirmPassword = "";
        public string ConfirmPassword
        {
            get => confirmPassword;
            set => confirmPassword = value.Trim();
        }
        private string inviteCode = "";
        public string InviteCode
        {
            get => inviteCode;
            set => inviteCode = value.ToUpper().Trim();
        }
        private string passphrase = "";
        public string Passphrase
        {
            get => passphrase;
            set => passphrase = value.Trim();
        }
        private byte[]? passphraseBytes = null;
        private string confirmPassphrase = "";
        public string ConfirmPassphrase
        {
            get => confirmPassphrase;
            set => confirmPassphrase = value.Trim();
        }

        public bool IsRegistering { get; set; } = false;
        public string? ErrorMessage { get; set; } = "";
        public bool IsBusy { get; set; } = false;

        public event Action? StateChanged;

        public LoginViewModel
            (IAuthenticationService authService,
            NavigationManager nav,
            AuthenticationStateProvider authStateProvider)
        {
            _authService = authService;
            _nav = nav;
            _authStateProvider = authStateProvider;
        }

        //validation
        private bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name) ||
            name.Length > 20 || name.Length < 5)
            {
                return false;
            }
            return true;
        }

        private bool IsPasswordValid(string password)
        {
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_-])[A-Za-z\d@$!%*?&_-]{12,30}$";
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, pattern);
        }

        private bool IsInviteCodeValid(string code)
        {
            if (code.Any(c => !char.IsLetterOrDigit(c)))
                return false;
            if (code.Length != 12)
                return false;

            return true;
        }

        private bool IsConfirmPasswordValid(string confirmPassword) => Password == confirmPassword;


        private bool IsPassphraseValid(string passphrase)
        {
            if (string.IsNullOrWhiteSpace(passphrase)
            || passphrase.Length < 16 || passphrase.Length > 256)
            {
                return false;
            }
            return true;
        }

        private bool IsConfirmPassphraseValid(string confirmPassphrase) => Passphrase == confirmPassphrase;

        private bool Validate()
        {
            if (!IsNameValid(Username))
            {
                ErrorMessage = "Username invalid. Username must be between 5 and 20 characters long.";
                return false;
            }
            if (!IsPasswordValid(Password))
            {
                ErrorMessage = "Password invalid. Password must be between 12 and 30 characters long. Password must contain at least one number, uppercase letter, lowercase letter and special character (@$!%*?&_-)";
                return false;
            }

            if (IsRegistering)
            {
                if (!IsInviteCodeValid(InviteCode))
                {
                    ErrorMessage = "Invite code invalid. Invite code must be 12 characters long and contain only letters and numbers";
                    return false;
                }
                if (!IsConfirmPasswordValid(ConfirmPassword))
                {
                    ErrorMessage = "Passwords don't match.";
                    return false;
                }
                if (!IsPassphraseValid(Passphrase))
                {
                    ErrorMessage = "Pasphrase invalid. Passphrase must be 16-256 characters long";
                }
                if (!IsConfirmPassphraseValid(ConfirmPassphrase))
                {
                    ErrorMessage = "Passphrases don't match.";
                    return false;
                }
            }
            return true;
        }

        //methods

        public void ToggleMode()
        {
            IsRegistering = !IsRegistering;
            ErrorMessage = null;
            ConfirmPassword = "";
            InviteCode = "";
            Notify();
        }

        public async Task SubmitAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = null;

            bool success = false;

            if (!Validate())
            {
                IsBusy = false;
                Console.WriteLine(ErrorMessage);
                Notify();
                return;
            }

            try
            {
                passwordBytes = Encoding.UTF8.GetBytes(Password);

                if (IsRegistering)
                {

                    passphraseBytes = Encoding.UTF8.GetBytes(passphrase);
                    success = await _authService.RegisterAsync(Username, passwordBytes, InviteCode, passphraseBytes);
                }
                else
                {
                    success = await _authService.LoginAsync(Username, passwordBytes);
                }

                IsBusy = false;

                if (success)
                {
                    Passphrase = string.Empty;
                    _nav.NavigateTo("/");
                    ((HostAuthenticationStateProvider)_authStateProvider).NotifyStateChanged();
                }
                else
                {
                    ErrorMessage = IsRegistering ? "Registration failed." : "Invalid credentials or login failed.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Exception: {ex.Message}";
            }
            finally
            {
                if (passphraseBytes != null) CryptographicOperations.ZeroMemory(passphraseBytes);
                if (passwordBytes != null) CryptographicOperations.ZeroMemory(passwordBytes);
                Notify();
            }
        }

        public async Task InitializeAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                _nav.NavigateTo("/");
            }
        }


        public void Notify() => StateChanged?.Invoke();

        public void Dispose()
        {
            if (passphraseBytes != null) CryptographicOperations.ZeroMemory(passphraseBytes);
            if (passwordBytes != null) CryptographicOperations.ZeroMemory(passwordBytes);
            Passphrase = string.Empty;
            ConfirmPassphrase = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}