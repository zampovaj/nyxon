using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace Nyxon.Client.ViewModels
{
    public class SettingsViewModel : IDisposable
    {
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
        public bool Success { get; set; } = false;

        public event Action? StateChanged;

        public SettingsViewModel(IAccountManagementService accountManagementService)
        {
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
        private bool IsNewPasswordValid(string password)
        {
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_-])[A-Za-z\d@$!%*?&_-]{12,30}$";
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, pattern);
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
            ErrorMessage = "";
            if (!IsPasswordValid(Password))
            {
                ErrorMessage = "Invalid password input detected. Password must be between 12 and 30 characters long.";
                return;
            }
            try
            {
                passwordBytes = Encoding.UTF8.GetBytes(Password);

                await _accountManagementService.DeleteAccountAsync(passwordBytes);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                if (passwordBytes != null) CryptographicOperations.ZeroMemory(passwordBytes);
                Clear();
                Notify();
            }
        }

        public async Task ChangePasswordAsync()
        {
            ErrorMessage = "";
            if (!IsPasswordValid(Password) || !IsNewPasswordValid(NewPassword))
            {
                ErrorMessage = "Password invalid. Password must be between 12 and 30 characters long. Password must contain at least one number, uppercase letter, lowercase letter and special character (@$!%*?&_-)";
                return;
            }

            if (!IsConfirmPasswordValid(ConfirmNewPassword))
            {
                ErrorMessage = "Passwords don't match.";
                return;
            }

            try
            {
                passwordBytes = Encoding.UTF8.GetBytes(Password);
                newPasswordBytes = Encoding.UTF8.GetBytes(NewPassword);

                await _accountManagementService.ChangePasswordAsync(passwordBytes, newPasswordBytes);

                Success = true;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                Success = false;
                ErrorMessage = $"Failed to update password: {ex.Message}";
            }
            finally
            {
                if (passwordBytes != null) CryptographicOperations.ZeroMemory(passwordBytes);
                if (newPasswordBytes != null) CryptographicOperations.ZeroMemory(newPasswordBytes);
                Clear();
                Notify();
            }
        }

        public void Notify() => StateChanged?.Invoke();

        public void Clear()
        {
            if (passwordBytes != null) CryptographicOperations.ZeroMemory(passwordBytes);
            if (newPasswordBytes != null) CryptographicOperations.ZeroMemory(newPasswordBytes);
            Password = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }

        public void ClearMessages()
        {
            ErrorMessage = string.Empty;
            Success = false;
        }

        public void Dispose()
        {
            ErrorMessage = string.Empty;
            Clear();
        }
    }
}