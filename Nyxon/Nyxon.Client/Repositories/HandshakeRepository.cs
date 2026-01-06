using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Repositories
{
    public class HandshakeRepository : IHandshakeRepository
    {
        private readonly IApiService _apiService;

        public HandshakeRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> DeleteHandshakeAsync(Guid handshakeId)
        {
            try
            {
                await _apiService.DeleteAsync($"api/handshake/{handshakeId}");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}