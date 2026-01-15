using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IInviteCodeService
    {
        Task<Guid> ValidateAsync(string code);
        Task MarkUsedAsync(Guid id);
        Task<List<string>> CreateInvitesAsync(int count = 1);
    }
}