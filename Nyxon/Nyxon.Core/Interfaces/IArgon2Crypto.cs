using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IArgon2Crypto
    {
        byte[] DeriveKey(string passphrase, byte[] salt, int length);
    }


}