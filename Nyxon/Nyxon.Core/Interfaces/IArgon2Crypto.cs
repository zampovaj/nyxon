using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IArgon2Crypto
    {
        byte[] DeriveKey(byte[] passphrase, byte[] salt, int length);
        byte[] HashPassword(byte[] password, byte[] salt, int length);
        byte[] Hash(byte[] data, byte[] salt, int length, int degreeOfParallelism, int iterations, int memorySize);
    }


}