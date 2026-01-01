using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IHandshakeService
    {
        Task<List<HandshakeDto>> GetPendingHandshakesAsync(Guid userId);
        Task ClearHandshakesAsync();
        Task<bool> DeleteHandshakeAsync(Guid handshakeId);
    }
}