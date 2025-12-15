using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface ISessionIdService
    {
        Task<string> SaveSessionIdAsync(Guid userId);
        Task<string?> GetSessionIdAsync(string userId);
    }
}