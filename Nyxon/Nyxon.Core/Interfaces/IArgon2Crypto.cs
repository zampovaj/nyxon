using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IArgon2Crypto
    {
        byte[] DeriveKey(byte[] passphrase, byte[] salt, int length);
        byte[] HashPassword(string password, byte[] salt, int length);
        byte[] Hash(string text, byte[] salt, int length, int degreeOfParallelism, int iterations, int memorySize);
    }


}