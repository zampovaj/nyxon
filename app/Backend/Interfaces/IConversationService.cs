using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Interfaces
{
    public interface IConversationService
    {
        public Task<Guid> CreateConversationAsync(Guid initiatorId, string targetUsername);
    }
}