using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IAccountService
    {
        Task DeleteAccountAsync(Guid userId, byte[] passwordHash);
        Task<DateTime> GetJoinDateAsync(Guid userId);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}