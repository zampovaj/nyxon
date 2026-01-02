using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Nyxon.Client.Services
{
    public class HandshakeService : IHandshakeService
    {
        private readonly IHandshakeRepository _repository;

        public List<Handshake> Handshakes { get; private set; }
        public event Action OnChange;

        public HandshakeService(IHandshakeRepository repository)
        {
            _repository = repository;
        }

        public async Task UseAsync(Guid handshakeId)
        {
            Handshakes.RemoveAll(h => h.Id == handshakeId && !h.IsProcessing);
            

            var response = await _repository.DeleteHandshakeAsync(handshakeId);

            if (!response) throw new Exception("Couldn't delete handshake from database");

            NotifyStateChanged();
        }
        public async Task LoadHandshakesAsync(List<HandshakeDto> dtos)
        {
            Handshakes = dtos.Select(d => new Handshake(
                id: d.Id,
                conversationId: d.ConversationId,
                publicEphemeralKey: d.PublicEphemeralKey,
                publicIdentityKey: d.PublicIdentityKey,
                privateSpk: d.PrivateSpk,
                privateOpk: d.PrivateOpk
            )).ToList();
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}