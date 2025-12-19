using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Services.Helpers
{
    public class RandomService : IRandomService
    {
        public byte[] GenerateRandomBytes(int length)
        {
            byte[] buffer = new byte[length];
            RandomNumberGenerator.Fill(buffer);
            return buffer;
        }
    }
}