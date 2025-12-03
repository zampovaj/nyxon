using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.DTOs;

namespace Backend.Interfaces
{
    public interface IUserVaultService
    {
        public Task<UserVaultDto?> GetVaultAsync(Guid userId);
    }
}