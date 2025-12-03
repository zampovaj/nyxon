using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Interfaces
{
    public interface IJwtService
    {
        public string GenerateToken(Guid userId, string username);
    }
}