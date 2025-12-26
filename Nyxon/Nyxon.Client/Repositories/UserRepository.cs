using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IApiService _apiService;

        public UserRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<UserListDto>?> FetchUserListAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<UserListDto>>("api/user/list");
            }
            catch
            {
                return null;
            }
        }
    }
}