using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IHandshakeService : IDisposable
    {
        List<Handshake> Handshakes { get; }
        Task UseAsync(Guid handshakeId);
        Task LoadHandshakesAsync(List<HandshakeDto> dtos);
        event Action OnChange;
        public void Clear();
    }
}