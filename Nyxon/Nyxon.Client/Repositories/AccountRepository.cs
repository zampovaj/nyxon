using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Repositories
{
    public class AccountRepository : IAccountRepository
    {

        private readonly IApiService _apiService;

        public AccountRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<AccountMetadataDto?> GetAccountDataAsync()
        {
            try
            {
                return await _apiService.GetAsync<AccountMetadataDto>("api/account/data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during creating new invite codes: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                await _apiService.PostAsync<ChangePasswordRequest>("api/account/change-password", request);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during creating new invite codes: {ex.Message}");
                return false;
            }
        }
        public async Task DeleteAccountAsync(DeleteAccountRequest request)
        {
            try
            {
                await _apiService.PostAsync<DeleteAccountRequest>("api/account/delete", request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during creating new invite codes: {ex.Message}");
            }
        }
        public async Task<List<string>> GenerateInviteCodesAsync(int count = 1)
        {
            try
            {
                var dto = new NewInviteCodesRequest(count);
                var response = await _apiService.PostAsync<NewInviteCodesResponse, NewInviteCodesRequest>("api/invites", dto);
                return response.Codes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during creating new invite codes: {ex.Message}");
                return null;
            }
        }
    }
}