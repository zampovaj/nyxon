using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.ViewModels
{
    public class LoginViewModel
    {
        public readonly IAuthenticationService _authService;
        public readonly NavigationManager _nav;
        public readonly AuthenticationStateProvider _authStateProvider;

        [Required]
        public string Username { get; set; } = "";
        [Required]
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string InviteCode { get; set; } = "";

        public bool IsRegistering { get; set; } = false;
        public string? ErrorMessage { get; set; } = "";
        public bool IsBusy { get; set; } = false;

        public event Action? StateChanged;

        public LoginViewModel(IAuthenticationService authService, NavigationManager nav, AuthenticationStateProvider authStateProvider)
        {
            _authService = authService;
            _nav = nav;
            _authStateProvider = authStateProvider;
        }

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

            if (IsRegistering)
            {
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords dont match.";
                    IsBusy = false;
                    Notify();
                    return;
                }
                success = await _authService.RegisterAsync(Username, Password, InviteCode);
            }
            else
            {
                success = await _authService.LoginAsync(Username, Password);
            }

            IsBusy = false;

            if (success)
            {
                _nav.NavigateTo("/");
                ((HostAuthenticationStateProvider)_authStateProvider).NotifyStateChanged();
            }
            else
            {
                ErrorMessage = IsRegistering ? "Registration failed." : "Invalid credentials or login failed.";
            }
            Notify();

        }

        public void Notify() => StateChanged?.Invoke();
    }
}