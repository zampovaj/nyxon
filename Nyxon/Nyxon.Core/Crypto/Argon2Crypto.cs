using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Konscious.Security.Cryptography;

namespace Nyxon.Core.Crypto
{
    public class Argon2Crypto : IArgon2Crypto
    {
        //number of threads
        private const int degreeOfParallelism = 4;
        private const int iterations = 4;
        //64 bytes
        //memory used by iteration
        private const int memorySize = 64 * 1024;

        public byte[] DeriveKey(string passphrase, byte[] salt, int length)
        {
            using var argon2 = new Konscious.Security.Cryptography.Argon2id(Encoding.UTF8.GetBytes(passphrase));

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = degreeOfParallelism;
            argon2.Iterations = iterations;
            argon2.MemorySize = memorySize;

            return argon2.GetBytes(length);
        }
    }
}