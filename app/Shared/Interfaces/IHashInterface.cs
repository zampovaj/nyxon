using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IHashInterface
    {
        string HashInviteCode(string code);
    }
}