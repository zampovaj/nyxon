using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Interfaces
{
    public interface ILoginService
    {
        public Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}