using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces.Crypto
{
    public interface IArgon2Crypto
    {
        Task<byte[]> DeriveKeyAsync(
            byte[] passphrase,
            byte[] salt,
            int length,
            int iterations,
            int memoryKb,
            int parallelism);
    }
}