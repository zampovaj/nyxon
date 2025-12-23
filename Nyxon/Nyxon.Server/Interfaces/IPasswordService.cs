using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Server.Interfaces
{
    public interface IPasswordService
    {
        byte[] HashPassword(byte[] password, byte[] salt);
        bool VerifyPassword(byte[] password, byte[] salt, byte[] expectedHash);
    }
}