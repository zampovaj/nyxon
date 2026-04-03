using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IUserService
    {
        Task<List<UserListDto>> GetAllUsersButMeAsync(Guid userId);
        Task DeleteAccountAsync(Guid userId);
        Task<DateTime> GetJoinDate(Guid userId);
    }
}