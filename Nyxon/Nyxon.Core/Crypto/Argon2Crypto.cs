using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Konscious.Security.Cryptography;
using NSec.Cryptography;

namespace Nyxon.Core.Crypto
{
    public class Argon2Crypto : IArgon2Crypto
    {
        public byte[] DeriveKey(byte[] passphrase, byte[] salt, int length)
        {
            using var argon2 = new Konscious.Security.Cryptography.Argon2id(passphrase);

            argon2.Salt = salt;
            // only 1 thread to mitigate gpu powered attacks
            argon2.DegreeOfParallelism = 1;
            argon2.Iterations = 4;
            //strict settings cause passphrase is the key to everyhitng
            argon2.MemorySize = 256 * 1024;

            return argon2.GetBytes(length);
        }

        public byte[] HashPassword(byte[] password, byte[] salt, int length)
        {
            using var argon2 = new Konscious.Security.Cryptography.Argon2id(password);

            argon2.Salt = salt;
            // only 1 thread to mitigate gpu powered attacks
            argon2.DegreeOfParallelism = 1;
            argon2.Iterations = 2;
            argon2.MemorySize = 64 * 1024;

            return argon2.GetBytes(length);
        }

        public byte[] Hash(byte[] data, byte[] salt, int length, int degreeOfParallelism, int iterations, int memorySize)
        {
            using var argon2 = new Konscious.Security.Cryptography.Argon2id(data);

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = degreeOfParallelism;
            argon2.Iterations = iterations;
            argon2.MemorySize = memorySize * 1024;

            return argon2.GetBytes(length);
        }
    }
}