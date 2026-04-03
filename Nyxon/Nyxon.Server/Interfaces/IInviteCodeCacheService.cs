using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IInviteCodeCacheService
    {
        Task<int> GetInviteCodesCountAsync(Guid userId);
        Task SaveInviteAsync(Guid userId, byte[] hash);
        Task<bool> SaveInvitesAsync(Guid userId, List<byte[]> hashes);
        Task<Guid?> ValidateInviteCodeAsync(byte[] hash);
        Task DeleteInviteCodeAsync(Guid userId, byte[] hash);
        Task DeleteInvitesForUser(Guid userId);
    }
}