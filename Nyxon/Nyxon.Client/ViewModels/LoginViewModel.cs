using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.ViewModels
{
    public class LoginViewModel
    {
        private readonly IAuthenticationService _authService;
        private readonly NavigationManager _nav;

        public string Username { get; set; } = "FinalBoss_01";
        public string Password { get; set; } = "password123";
        public string InviteCode { get; set; } = "";

        public bool IsRegistering { get; private set; } = false;
        public string? ErrorMessage { get; private set; }
        public bool IsBusy { get; private set; }

        public LoginViewModel(IAuthenticationService authService, NavigationManager nav)
        {
            _authService = authService;
            _nav = nav;
        }

        public void ToggleMode()
        {
            IsRegistering = !IsRegistering;
            ErrorMessage = null;
        }

        public async Task SubmitAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = null;

            bool success = false;

            if (IsRegistering)
            {
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
            }
            else
            {
                ErrorMessage = IsRegistering ? "Registration failed." : "Invalid credentials or login failed.";
            }

        }
    }
}