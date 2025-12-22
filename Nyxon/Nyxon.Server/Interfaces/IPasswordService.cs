using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IPasswordService
    {
        byte[] HashPassword(string password, byte[] salt);
        bool VerifyPassword(string password, byte[] salt, byte[] expectedHash);
    }
}