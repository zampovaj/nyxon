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
    }
}