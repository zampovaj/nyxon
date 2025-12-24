using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.DTOs;

namespace Nyxon.Server.Interfaces
{
    public interface IUserVaultService
    {
        public Task<UserVaultResponse?> GetVaultAsync(Guid userId);
    }
}