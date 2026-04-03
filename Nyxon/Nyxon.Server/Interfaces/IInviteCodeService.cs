using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IInviteCodeService
    {
        Task<Guid?> ValidateAsync(byte[] hash);
        Task MarkUsedAsync(Guid userId, byte[] hash);
        Task<List<string>> CreateInvitesAsync(Guid userId, int count = 1);
        Task<int> GetInviteCodesCountAsync(Guid userId);
    }
}