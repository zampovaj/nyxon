using Nyxon.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface ILoginService
    {
        public Task<User?> LoginAsync(LoginRequest request);
    }
}