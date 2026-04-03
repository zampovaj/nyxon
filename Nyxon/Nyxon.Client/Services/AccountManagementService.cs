using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class AccountManagementService : IAccountManagementService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly UserContext _userContext;
        private readonly ICryptoService _cryptoService;

        public AccountManagementService(
            IAccountRepository accountRepository,
            UserContext userContext,
            ICryptoService cryptoService)
        {
            _accountRepository = accountRepository;
            _userContext = userContext;
            _cryptoService = cryptoService;
        }

        public async Task<AccountMetadataDto> FetchAccountDataAsync()
        {            
            var data = await _accountRepository.GetAccountDataAsync();
            if (data == null)
                throw new Exception("Failed to fetch account information");
            
            return data;
        }

        public async Task<bool> ChangePasswordAsync(byte[] currentPasswordBytes, byte[] newPasswordBytes)
        {
            if (!_userContext.IsAuthenticated)
                throw new UnauthorizedAccessException("Can't change password unless authenticated");

            byte[] newPasswordSalt = _cryptoService.GeneratePasswordSalt();
            byte[] currentPasswordHash = await _cryptoService.PreHashPasswordAsync(currentPasswordBytes, _userContext.Username);
            byte[] newPasswordHash = await _cryptoService.PreHashPasswordAsync(newPasswordBytes, _userContext.Username);

            var request = new ChangePasswordRequest
            {
                CurrentPasswordHash = currentPasswordHash,
                NewPasswordHash = newPasswordHash,
                NewPasswordSalt = newPasswordSalt
            };

            return await _accountRepository.ChangePasswordAsync(request);
        }
        public async Task DeleteAccountAsync(byte[] passwordBytes)
        {
            if (!_userContext.IsAuthenticated)
                throw new UnauthorizedAccessException("Can't change password unless authenticated");

            byte[] passwordHash = await _cryptoService.PreHashPasswordAsync(passwordBytes, _userContext.Username);
            var request = new DeleteAccountRequest {PasswordHash = passwordHash};

            await _accountRepository.DeleteAccountAsync(request);
        }
    }
}