using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Nyxon.Client.ViewModels
{
    public class SettingsViewModel : IDisposable
    {
        private readonly UserContext _userContext;
        private readonly NavigationManager _nav;
        private readonly IAccountManagementService _accountManagementService;

        [Required]
        private string password = "";
        public string Password
        {
            get => password;
            set => password = value.Trim();
        }
        private byte[]? passwordBytes = null;

        private string newPassword = "";
        public string NewPassword
        {
            get => newPassword;
            set => newPassword = value.Trim();
        }
        private byte[]? newPasswordBytes = null;

        private string confirmNewPassword = "";
        public string ConfirmNewPassword
        {
            get => confirmNewPassword;
            set => confirmNewPassword = value.Trim();
        }

        private int invitesCount = 0;
        public int InvitesCount
        {
            get => invitesCount;
            set
            {
                if (value < 0 || value > 20)
                    return;
                invitesCount = value;
            }
        }
        public readonly int MaxInvites = 20;
        public double InvitesPercentage => Math.Round((double)InvitesCount / MaxInvites * 100, 2);

        private DateTime joined = DateTime.UtcNow;
        public DateTime JoinedAt
        {
            get => joined;
            set
            {
                if (value > DateTime.UtcNow || value < new DateTime(2026, 1, 1))
                    return;
                joined = value;
            }
        }
        public string? ErrorMessage { get; set; } = "";

        public event Action? StateChanged;

        public SettingsViewModel(UserContext userContext,
            NavigationManager nav,
            IAccountManagementService accountManagementService)
        {
            _userContext = userContext;
            _nav = nav;
            _accountManagementService = accountManagementService;
        }

        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password) ||
            password.Length > 30 || password.Length < 12)
            {
                return false;
            }
            return true;
        }
        private bool IsConfirmPasswordValid(string confirmPassword) => NewPassword == confirmPassword;

        public async Task FetchAccountDataAsync()
        {
            try
            {
                var data = await _accountManagementService.FetchAccountDataAsync();

                JoinedAt = data.JoinedAt;
                InvitesCount = data.InvitesCount;

                Notify();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Notify();
            }
        }

        public async Task DeleteAccountAsync()
        {
            if (!IsPasswordValid(Password))
            {
                ErrorMessage = "Invalid input for password detected.";
            }

            // TODO:
            // call to account managent service
            // if successful, return 401 -> gets handled automatically
        }

        public async Task ChangePasswordAsync()
        {
            if (!IsPasswordValid(Password))
            {
                ErrorMessage = "Invalid input for password detected.";
                Notify();
                return;
            }

            if (!IsConfirmPasswordValid(ConfirmNewPassword))
            {
                ErrorMessage = "Passwords don't match.";
                Notify();
                return;
            }

            try
            {
                passwordBytes = Encoding.UTF8.GetBytes(Password);
                newPasswordBytes = Encoding.UTF8.GetBytes(NewPassword);

                await _accountManagementService.ChangePasswordAsync(passwordBytes, newPasswordBytes);

                // TODO:
                // display the result to user
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update password: {ex.Message}";
            }
            finally
            {
                if (passwordBytes != null) CryptographicOperations.ZeroMemory(passwordBytes);
                if (newPasswordBytes != null) CryptographicOperations.ZeroMemory(newPasswordBytes);
                Notify();
            }
        }

        public void Notify() => StateChanged?.Invoke();

        public void Dispose()
        {
            if (passwordBytes != null) CryptographicOperations.ZeroMemory(passwordBytes);
            if (newPasswordBytes != null) CryptographicOperations.ZeroMemory(newPasswordBytes);
            Password = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }
    }
}