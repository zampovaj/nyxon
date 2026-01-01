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
                var response = await _apiService.DeleteAsync<bool>($"api/handshake/{handshakeId}");
                return response;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}