using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces
{
    public interface IRandomService
    {
        byte[] GenerateRandomBytes(int length);
    }
}