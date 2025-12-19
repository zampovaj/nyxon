using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Crypto
{
    public class RandomService : IRandomService
    {
        public byte[] GenerateRandomBytes(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte[] buffer = new byte[length];
            RandomNumberGenerator.Fill(buffer);
            return buffer;
        }
    }
}