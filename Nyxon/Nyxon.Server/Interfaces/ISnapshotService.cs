using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface ISnapshotService
    {
        Task SaveNewAsync(Guid userId, CreateSnapshotDto snapshotDto);
        Task<SnapshotsDto> GetSnapshotsAsync(Guid userId, Guid conversationId, List<MessageResponse> messages);
    }
}