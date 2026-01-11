using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Repositories
{
    public class VaultRepository : IVaultRepository
    {
        private readonly IApiService _apiService;

        public VaultRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<UserVaultResponse?> FetchUserVaultAsync()
        {
            try
            {
                return await _apiService.GetAsync<UserVaultResponse>("api/user/vault");
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveNewSpkAsync(SignedPrekey spk)
        {
            try
            {
                var dto = new SignedPrekeyDto(spk);
                await _apiService.PostAsync<SignedPrekeyDto>("api/prekeys", dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving new signed prekey: {ex.Message}");
            }
        }

        public async Task<bool> CheckSignedPrekeyAsync()
        {
            try
            {
                return await _apiService.GetAsync<bool>("api/prekeys");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking the expiration date of signed prekey");
                return false;
            }
        }

    }
}